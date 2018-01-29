using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompilerFinal.Writer.Contracts
{
    public interface IVmWriter
    {
        void WritePush(string segment, int index);
        void WritePop(string segment, int index);
        void WriterArithmetic(string command);
        void WriteLabel(string label);
        void WriteGoto(string label);
        void WriteIfGoto(string label);
        void WriteCall(string name, int nArgs);
        void WriteFunction(string name, int nLocals);
        void WriteReturn();
        void WriteOp(Token opToken);
        void WriteUnaryOp(Token opToken);

    }
}
