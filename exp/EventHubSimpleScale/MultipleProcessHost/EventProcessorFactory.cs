using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleProcessHost
{
    public class EventProcessorFactory : IEventProcessorFactory
    {
        protected int flag = -1;

        public EventProcessorFactory(int i)
        {
            this.flag = i;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new SimpleEventProcessor(this.flag);
        }
    }
}
