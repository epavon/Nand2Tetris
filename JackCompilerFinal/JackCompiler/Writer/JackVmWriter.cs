using JackCompiler.Writer.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Writer
{
    public class JackVmWriter : IVmWriter, IDisposable
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
            _streamWriter.WriteLine("push " + segment + " " + index);
        }

        public void WritePop(string segment, int index)
        {
            _streamWriter.WriteLine("pop " + segment + " " + index);
        }

        public void WriterArithmetic(string command)
        {
            _streamWriter.WriteLine(command);
        }

        public void WriteLabel(string label)
        {
            _streamWriter.WriteLine("label " + label);
        }

        public void WriteGoto(string label)
        {
            _streamWriter.WriteLine("goto " + label);
        }

        public void WriteIfGoto(string label)
        {
            _streamWriter.WriteLine("if-goto " + label);
        }

        public void WriteCall(string name, int nArgs)
        {
            _streamWriter.WriteLine("call " + name + " " + nArgs);
        }

        public void WriteFunction(string name, int nLocals)
        {
            _streamWriter.WriteLine("function " + name + " " + nLocals);
        }

        public void WriteReturn()
        {
            _streamWriter.WriteLine("return");
        }

        public void WriteOp(Token opToken)
        {
            string opCommand = string.Empty;
            switch(opToken.Value)
            {
                case "+":
                    opCommand = "add";
                    break;
                case "-":
                    opCommand = "sub";
                    break;
                case "*":
                    opCommand = "call Math.multiply 2";
                    break;
                case "<":
                    opCommand = "lt";
                    break;
                case ">":
                    opCommand = "gt";
                    break;
                case "=":
                    opCommand = "eq";
                    break;
                case "&":
                    opCommand = "and";
                    break;
                case "|":
                    opCommand = "or";
                    break;
                case "/":
                    opCommand = "call Math.divide 2";
                    break;
                default:
                    break;

            }
            _streamWriter.WriteLine(opCommand);
        }




        public void WriteUnaryOp(Token opToken)
        {
            _streamWriter.WriteLine(opToken.Value == "-" ? "neg" : "not");
        }
    }
}
