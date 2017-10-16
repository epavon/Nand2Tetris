using JackCompiler.Types;
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

        public object CurrentToken { get; private set; }


        //
        // Ctors / Dtors
        //
        public Tokenizer(string jackFilePath)
        {
            _reader = new StreamReader(jackFilePath);
        }

        ~Tokenizer()
        {
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }

        //
        // Methods
        //

        //
        // Skip over whitespace and return whether there is more to read from stream
        public bool HasMoreTokens()
        {
            bool result = false;
            char curChar = ' ';

            // Skip spaces, tabs, end-of-line, ...
            for (; _reader.Peek() >= 0; curChar = (char)_reader.Peek())
            {
                switch (curChar)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        _reader.Read();
                        continue;
                }

                result = true;
                break;
            }

            return result;
        }

        //
        // Set CurrentToken and advance stream reader
        public void Advance()
        {
            string token = string.Empty;
            char curChar = ' ', nextChar = ' ';
            bool commentBlock = false, isStringConstant = false, isIntegerConstant = false;

            for (; _reader.Peek() >= 0; curChar = (char)_reader.Read(), nextChar = (char)_reader.Peek())
            {
                // handle block comment
                if(commentBlock)
                {
                    if(curChar == '*' && nextChar == '/')
                    {
                        commentBlock = false;
                    }
                    _reader.Read();
                    continue;
                }

                // handle symbol
                if(SettingsReader.Symbols.Contains(curChar))
                {
                    // Is it comment?
                    if (curChar == '/')
                    {
                        if (nextChar == '/')
                        {
                            token = string.Empty;
                            _reader.ReadLine();
                            continue;
                        }
                        else if(nextChar == '*')
                        {
                            commentBlock = true;
                            continue;
                        }
                    }

                    // It's not a comment
                    CurrentToken = new Token<char>
                    {
                        Value = curChar,
                        TokenType = TokenType.SYMBOL
                    };
                    break;
                }

                // handle StringConstant
                if (curChar == '"')
                {
                    if (isStringConstant)
                    {
                        CurrentToken = new Token<string>
                        {
                            TokenType = TokenType.STRING_CONST,
                            Value = token
                        };
                        break;
                    }
                    else
                    {
                        isStringConstant = true;
                        continue;
                    }
                }

                // Handle integerConstant
                if(Char.IsDigit(curChar))
                {
                    isIntegerConstant = true;
                }

                token += curChar;
                if(isStringConstant || isIntegerConstant)
                {
                    continue;
                }

                // handle keyword
                if(SettingsReader.Keywords.Contains(token) && nextChar == ' ')
                {
                    CurrentToken = new Token<string>
                    {
                        Value = token,
                        TokenType = TokenType.KEYWORD
                    };
                    break;
                }

                // handle identifier 
                if(nextChar == ' ' || nextChar == '\t' || nextChar == '\r' || nextChar == '\n' || SettingsReader.Symbols.Contains(nextChar))
                {
                    if (IdentifierHelper.IdentifierRegex.IsMatch(token))
                    {

                    }
                }
                
                
            }

            if(CurrentToken == null)
            {   // Error out?

            }

        }

        


    }
}
