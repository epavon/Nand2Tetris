using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Exceptions
{
    public class BadSyntaxException : Exception
    {
        public BadSyntaxException() : base("Bad Syntax Exception")
        {

        }
    }
}
