using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SimpleTestData : IEventHubMessageObject
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public DateTime TimeStamp { get; set; }

        public Microsoft.ServiceBus.Messaging.EventData ToEventData()
        {
            var jsonStr = JsonConvert.SerializeObject(this);
            return new EventData(Encoding.UTF8.GetBytes(jsonStr));
        }
    }
}
