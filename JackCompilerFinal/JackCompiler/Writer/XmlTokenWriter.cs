using JackCompiler.Contracts;
using JackCompiler.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    public class XmlTokenWriter : ITokenWriter, IDisposable
    {
        StreamWriter _streamWriter;

        public XmlTokenWriter(string outputFile)
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

        //
        // Methods
        //

        public void WriteTokenStart(string compUnit, int depth)
        {
            string initSpaces = WriterHelper.GetSpacesDepth(depth);
            _streamWriter.WriteLine(initSpaces + "<" + compUnit + ">");
        }

        public void WriteTokenEnd(string compUnit, int depth)
        {
            string initSpaces = WriterHelper.GetSpacesDepth(depth);
            _streamWriter.WriteLine(initSpaces + "</" + compUnit + ">");
        }

        public void WriteTerminalToken(Token token, int depth)
        {
            string initSpaces = WriterHelper.GetSpacesDepth(depth);
            _streamWriter.Write(initSpaces + "<" + token.GetTokenTypeName() + "> ");
            _streamWriter.Write(token.OutputValue);
            _streamWriter.WriteLine(" </" + token.GetTokenTypeName() + ">");
        }

        
    }
}
