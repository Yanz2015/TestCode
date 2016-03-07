using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMediaIntelligence.Business.Models.Query
{
    public enum TokenType
    {
        Bracket,
        Operator, 
        Expression, 
    }

    public class Token
    {
        public string Text { get; set; }

        public TokenType Type { get; set; }

    }
}
