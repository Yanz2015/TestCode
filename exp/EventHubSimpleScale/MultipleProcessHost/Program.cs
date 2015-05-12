using Microsoft.ServiceBus.Messaging;
using Microsoft.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleProcessHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = Directory.GetFiles(@".\", "*.outx");
            foreach (var file in files)
            {
                File.Delete(file);
            }

            Writer.CreateWriter();
            AsyncPump.Run(MainAsync);
            Console.ReadKey();
            Writer.Dispose();
        }

        static async Task MainAsync()
        {
            string eventHubConnectionString = "Endpoint=sb://yazha-ns.servicebus.windows.net/;SharedAccessKeyName=listen;SharedAccessKey=MFg5Ef9ueC4tYcXHrMOR+a6p9XLCQYWBUrNxMxsSk04=";
            string eventHubName = "scalehubtest";
            string storageAccountName = "yazha";
            string storageAccountKey = "c4KmKhEIK0uR519ANRTIz50y7XtAdkB1nvrsy27C9JB1sajHrzY5McuEuufHuVgSqQAsj52tdv0igr2xOkD7PA==";
            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                storageAccountName, storageAccountKey);

            int count = 32;
            List<Task> taskList = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                var tmp = i;
                var task = Task.Run(async () =>
                {
                    string eventProcessorHostName = Guid.NewGuid().ToString();
                    EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
                    var factory = new EventProcessorFactory(tmp);
                    await eventProcessorHost.RegisterEventProcessorFactoryAsync(factory);
                });
                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());

            //var t1 = Task.Run(async () =>
            //{
            //    string eventProcessorHostName = Guid.NewGuid().ToString();
            //    EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
            //    await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();
            //});

            //var t2 = Task.Run(async () =>
            //{
            //    var eventProcessorHostName = Guid.NewGuid().ToString();
            //    EventProcessorHost eventProcessorHost2 = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
            //    var factory = new EventProcessorFactory();
            //    await eventProcessorHost2.RegisterEventProcessorFactoryAsync(factory);
            //});

            //Task.WaitAll(t1, t2);




            Console.WriteLine("Receiving. Press enter key to stop worker.");
        }
    }
}
