﻿using JackCompiler.Contracts;
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

        //
        // Methods
        //

        public void WriteTokenStart(Token token)
        {
            throw new NotImplementedException();
        }

        public void WriteTokenEnd(Token token)
        {
            throw new NotImplementedException();
        }

        public void WriteTerminalToken(Token token)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if(_streamWriter != null && _streamWriter.BaseStream != null)
            {
                _streamWriter.Dispose();
                _streamWriter = null;
            }
        }
    }
}
