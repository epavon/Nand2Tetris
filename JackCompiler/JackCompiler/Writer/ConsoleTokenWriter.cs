using JackCompiler.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Writer
{
    public class ConsoleTokenWriter : ITokenWriter
    {
        public void WriteTokenStart(string compUnit)
        {
            Console.WriteLine("<" + compUnit + ">");
        }

        public void WriteTokenEnd(string compUnit)
        {
            Console.WriteLine("</" + compUnit + ">");
        }

        public void WriteTerminalToken(string compUnit, Token token)
        {
            WriteTokenStart(compUnit);
            Console.WriteLine(token.Value);
            WriteTokenEnd(compUnit);
        }
    }
}
