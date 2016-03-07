using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    using MCMediaIntelligence.Business.Models.Query;

    class Program
    {
        static void Main(string[] args)
        {
            var result = char.IsLetterOrDigit('(');
            Console.Write(result);


            string test = "(name1 eq value1 and name2 eq value2 or name3 eq value3 and (name4 eq value4 or name5 eq value5))";

            FilterParser parser = new FilterParser();
            var collection = parser.ParseFilter(test);


            Console.Read();
        }
    }

}
