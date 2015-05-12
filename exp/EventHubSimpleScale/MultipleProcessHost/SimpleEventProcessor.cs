using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleProcessHost
{
    public class SimpleEventProcessor : IEventProcessor
    {
        public Stopwatch checkpointStopWatch;
        protected int flag = -1;

        public SimpleEventProcessor(int i)
        {
            this.flag = i;
        }

        async Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor{2} Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason, this.flag.ToString());
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        protected virtual string GetProcessorName()
        {
            return "SimpleEventProcessor" + this.flag;
        }

        public virtual Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine("{2} initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset, this.GetProcessorName());
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            return Task.FromResult<object>(null);
        }

        async Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                string data = Encoding.UTF8.GetString(eventData.GetBytes());

                try
                {
                    var sw = GetLogName();
                    lock (sw)
                    {
                        sw.WriteLine(string.Format("Message received.  Partition: '{0}', Data: '{1}'",
                            context.Lease.PartitionId, data));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //Call checkpoint every 5 minutes, so that worker can resume processing from the 5 minutes back if it restarts.
            if (this.checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(5))
            {
                await context.CheckpointAsync();
                this.checkpointStopWatch.Restart();
            }
        }

        protected virtual StreamWriter GetLogName()
        {
            return Writer.GetWriter(this.flag);
        }
    }

    public static class Writer
    {
        static List<StreamWriter> swList = new List<StreamWriter>();
        public static void CreateWriter()
        {
            for (int i = 0; i < 32; i++)
            {
                StreamWriter sw = new StreamWriter("Thread" + i.ToString()+".outx", true, Encoding.UTF8);
                swList.Add(sw);
            }
        }

        public static StreamWriter GetWriter(int i)
        {
            return swList[i];
        }


        public static void Dispose()
        {
            foreach (var sw in swList)
            {
                sw.Dispose();
            }
        }
    }
}
