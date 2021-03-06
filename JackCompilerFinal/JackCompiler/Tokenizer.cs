﻿using JackCompilerFinal.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompilerFinal
{
    public class Tokenizer : IDisposable
    {
        private StreamReader _reader;

        public Token CurrentToken { get; private set; }
        public Token NextToken { get; private set; }
        
        //
        // Ctors / Dtors
        //
        public Tokenizer(string jackFilePath)
        {
            _reader = new StreamReader(jackFilePath);
        }

        ~Tokenizer()
        {
            Dispose();
        }

        public void Dispose()
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
            char curChar = (char)_reader.Peek();

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
            CurrentToken = null;
            string token = string.Empty;

            if (NextToken != null)
            {
                CurrentToken = NextToken;
                NextToken = null;
                return;
            }

            bool commentBlock = false, isStringConstant = false;
            if (HasMoreTokens())
            {
                int curCharNum = 32;
                for (char curChar = ' ', nextChar = ' '; curCharNum >= 0; curCharNum = _reader.Read(), nextChar = (char)_reader.Peek())
                {
                    curChar = Convert.ToChar(curCharNum);
                    // Skip whitespace
                    if (!isStringConstant)
                    {
                        switch (curChar)
                        {
                            case ' ':
                            case '\t':
                            case '\r':
                            case '\n':
                                continue;
                        }
                    }

                    // build token
                    if (curChar != '"' && !commentBlock)
                        token += curChar;

                    // handle block comment
                    if (commentBlock)
                    {
                        if (curChar == '*' && nextChar == '/')
                        {
                            _reader.Read();
                            commentBlock = false;
                        }
                        continue;
                    }

                    // handle stringConstant
                    if (isStringConstant)
                    {
                        if (nextChar == '"')
                        {
                            _reader.Read();
                            CurrentToken = new Token
                            {
                                Value = token,
                                TokenType = TokenType.STRING_CONST
                            };
                            break;
                        }
                        if (curChar == '"')
                        {   // Empty string
                            CurrentToken = new Token
                            {
                                Value = token,
                                TokenType = TokenType.STRING_CONST
                            };
                            break;
                        }
                        continue;
                    }
                    else if (curChar == '"')
                    {
                        isStringConstant = true;
                        token = string.Empty;
                        continue;
                    }

                    // handle symbol
                    if (SettingsReader.Symbols.Contains(curChar))
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
                            else if (nextChar == '*')
                            {
                                token = string.Empty;
                                commentBlock = true;
                                continue;
                            }
                        }

                        // It's not a comment
                        CurrentToken = new Token
                        {
                            Value = curChar.ToString(),
                            TokenType = TokenType.SYMBOL
                        };
                        break;
                    }

                    // reached delimmitter 
                    if (nextChar == ' ' || nextChar == '\t' || nextChar == '\r' || nextChar == '\n' || nextChar == '"' || SettingsReader.Symbols.Contains(nextChar))
                    {
                        // handle keyword
                        if (SettingsReader.Keywords.Contains(token))
                        {
                            CurrentToken = new Token
                            {
                                Value = token,
                                TokenType = TokenType.KEYWORD
                            };
                            break;
                        }

                        // handle end of integerConstant
                        int num;
                        if (Int32.TryParse(token, out num))
                        {
                            CurrentToken = new Token
                            {
                                Value = num.ToString(),
                                TokenType = TokenType.INT_CONST
                            };
                            break;
                        }

                        // handle identifier
                        if (IdentifierHelper.IdentifierRegex.IsMatch(token))
                        {
                            CurrentToken = new Token
                            {
                                TokenType = TokenType.IDENTIFIER,
                                Value = token
                            };
                            break;
                        }

                        // Then we have an error - nothing to do for this ast
                        throw new Exception("Not a valid token");
                    }
                }
            }

        }

        //
        // Peek
        public Token Peek()
        {
            if(NextToken == null)
            {
                var tmp = CurrentToken;
                Advance();
                NextToken = CurrentToken;
                CurrentToken = tmp;
                return NextToken;
            }

            return NextToken;
        }

        
    }
}
