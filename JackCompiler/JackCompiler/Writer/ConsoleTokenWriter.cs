using JackCompiler.Contracts;
using JackCompiler.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Writer
{
    public class ConsoleTokenWriter : ITokenWriter
    {
        public void WriteTokenStart(string compUnit, int depth)
        {
            string initSpaces = WriterHelper.GetSpacesDepth(depth);
            Console.WriteLine(initSpaces + "<" + compUnit + ">");
        }

        public void WriteTokenEnd(string compUnit, int depth)
        {
            string initSpaces = WriterHelper.GetSpacesDepth(depth);
            Console.WriteLine(initSpaces + "</" + compUnit + ">");
        }

        public void WriteTerminalToken(Token token, int depth)
        {
            string initSpaces = WriterHelper.GetSpacesDepth(depth);
            Console.Write(initSpaces + "<" + token.GetTokenTypeName() + ">");
            Console.Write(token.OutputValue);
            Console.WriteLine("</" + token.GetTokenTypeName() + ">");
        }

        
    }
}
