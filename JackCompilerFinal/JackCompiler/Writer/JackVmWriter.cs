using JackCompiler.Writer.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Writer
{
    public class JackVmWriter : IVmWriter
    {
        StreamWriter _streamWriter;

        public JackVmWriter(string outputFile)
        {
            _streamWriter = new StreamWriter(outputFile);
        }

        public void Dispose()
        {
            if (_streamWriter != null && _streamWriter.BaseStream != null)
            {
                _streamWriter.Close();
                _streamWriter = null;
            }
        }

        public void WritePush(string segment, int index)
        {
            throw new NotImplementedException();
        }

        public void WritePop(string segment, int index)
        {
            throw new NotImplementedException();
        }

        public void WriterArithmetic(string command)
        {
            throw new NotImplementedException();
        }

        public void WriteLabel(string label)
        {
            throw new NotImplementedException();
        }

        public void WriteGoto(string label)
        {
            throw new NotImplementedException();
        }

        public void WriteIf(string label)
        {
            throw new NotImplementedException();
        }

        public void WriteCall(string name, int nArgs)
        {
            _streamWriter.WriteLine("call " + name + " " + nArgs);
        }

        public void WriteFunction(string name, int nLocals)
        {
            throw new NotImplementedException();
        }

        public void WriteReturn()
        {
            throw new NotImplementedException();
        }

        public void WriteOp(Token opToken)
        {
            throw new NotImplementedException();
        }
    }
}
