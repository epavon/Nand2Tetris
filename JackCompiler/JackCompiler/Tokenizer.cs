using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    public class Tokenizer
    {
        private StreamReader _reader;

        public Token CurrentToken { get; set; }
        public Token NextToken { get; set; }
        public TokenType CurrentTokenType { get; set; }
        public string[] InputLines { get; set; }
        

        public Tokenizer(string jackFilePath)
        {
            _reader = new StreamReader(jackFilePath);
        }

        ~Tokenizer()
        {
            if(_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }

        //
        // Methods
        public bool HasMoreTokens()
        {
            if(!_reader.EndOfStream)
            {
                return true;
            }

            return false;
        }

        public void Advance()
        {
            string token = string.Empty;
            while(true) {
                token += _reader.Read();

            }
        }

    }
}
