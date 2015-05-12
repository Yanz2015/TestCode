using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SimpleDataProducer
    {
        static Random rnd = new Random();
        public static SimpleTestData ProduceSimpleTestData()
        {
            var id = rnd.Next(1, 129);
            var timeStamp = DateTime.UtcNow;
            return new SimpleTestData
            { 
                Id = id, 
                TimeStamp = timeStamp, 
                Message = string.Format("Message Generated With Id {0} at {1}", id.ToString(), timeStamp.ToString())
            };
        }
    }
}
