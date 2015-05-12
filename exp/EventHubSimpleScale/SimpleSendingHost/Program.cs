using Common;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSendingHost
{
    class Program
    {
        static int counter = 0;
        static object locker = new object();
        static void Main(string[] args)
        {
            int threadCount = 100;
            
             var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            TaskFactory factory = new TaskFactory(token);
            for (var i = 0; i < threadCount; i++)
            {
                factory.StartNew(() => {
                    SendingData(token).Wait();
                });
            }


            Console.ReadKey();

            tokenSource.Cancel();
        }

        static async Task SendingData(CancellationToken token)
        {
            var connection = ConfigurationManager.AppSettings["EventHubString"];
            var ehName = ConfigurationManager.AppSettings["EventHubName"];

            HubSender sender = new HubSender(connection, ehName);
            await sender.InitializeAsync();
            int index = 0;
            while (!token.IsCancellationRequested)
            {
                while (true)
                {
                    try
                    {
                        var data = SimpleDataProducer.ProduceSimpleTestData();
                        await sender.SendMessage<SimpleTestData>(data);
                        break;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                lock (locker)
                {
                    counter++;
                }

                
                index++;
                if (index % 10 == 0)
                {
                    Console.WriteLine("{0} Processed", counter.ToString());
                }
            }
        }
    }
}
