using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Contracts
{
    public interface ITokenWriter
    {
        void WriteTokenStart(Token token);
        void WriteTokenEnd(Token token);
        void WriteTerminalToken(Token token);
    }
}
