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
        void WriteTokenStart    (string compUnit, int depth);
        void WriteTokenEnd      (string compUnit, int depth);
        void WriteTerminalToken (Token token, int depth);
    }
}
