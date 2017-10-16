using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    public class Token<T>
    {
        public T Value { get; set; }
        public TokenType TokenType { get; set; }
    }
}
