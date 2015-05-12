using Common;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSendingHost
{
    public class HubSender
    {
        public string Connection { get; set; }

        public string EHName { get; set; }

        public EventHubClient Client { get; set; }

        public HubSender(string connection, string ehName)
        {
            this.Connection = connection;
            this.EHName = ehName;
        }

        public async Task InitializeAsync()
        {
            this.Client = EventHubClient.CreateFromConnectionString(this.Connection, this.EHName);
            Task.FromResult(0);
        }

        public async Task SendMessage<T>(T obj) where T: IEventHubMessageObject
        {
            await this.Client.SendAsync(obj.ToEventData());
            Task.FromResult(0);
        }
    }
}
