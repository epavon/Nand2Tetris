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
        public void WriteTokenStart(string compUnit, int depth)
        {
            string initSpaces = GetSpacesDepth(depth);
            Console.WriteLine(initSpaces + "<" + compUnit + ">");
        }

        public void WriteTokenEnd(string compUnit, int depth)
        {
            string initSpaces = GetSpacesDepth(depth);
            Console.WriteLine(initSpaces + "</" + compUnit + ">");
        }

        public void WriteTerminalToken(Token token, int depth)
        {
            string initSpaces = GetSpacesDepth(depth);
            Console.Write(initSpaces + "<" + token.GetTokenTypeName() + ">");
            Console.Write(token.Value);
            Console.WriteLine("</" + token.GetTokenTypeName() + ">");
        }

        private string GetSpacesDepth(int depth)
        {
            string result = string.Empty;
            for (int i = 0; i < depth*2; i++)
            {
                result += " ";
            }
            return result;
        }
    }
}
