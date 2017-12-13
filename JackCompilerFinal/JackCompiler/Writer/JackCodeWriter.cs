using JackCompiler.Writer.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Writer
{
    public class JackCodeWriter : ICodeWriter
    {
        StreamWriter _streamWriter;

        public JackCodeWriter(string outputFile)
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

        public void WriteExpr(Token expr)
        {
            if(expr.TokenType == TokenType.INT_COSNT)
            {
                _streamWriter.WriteLine("push " + expr.Value);
            }
            else if(expr.TokenType == TokenType.IDENTIFIER)
            {

            }
            //else if(expr.TokenType == TokenType.)
        }

        public void WriteVar(string var)
        {
            throw new NotImplementedException();
        }
    }
}
