using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Writer.Contracts
{
    public interface ICodeWriter
    {
        void WriteExpr(Token expr);
        void WriteVar(string var);
    }
}
