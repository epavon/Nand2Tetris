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
        void WriteTokenStart    (string compUnit);
        void WriteTokenEnd      (string compUnit);
        void WriteTerminalToken (string compUnit, Token token);
    }
}
