using JackCompiler.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    public class CompilationEngine : IDisposable
    {
        Tokenizer       _tokenizer;
        ITokenWriter    _tokenWriter;

        //
        // Ctors / Dtors
        //
        public CompilationEngine(Tokenizer tokenizer, ITokenWriter tokenWriter)
        {
            _tokenizer      = tokenizer;
            _tokenWriter    = tokenWriter;
        }

        ~CompilationEngine()
        {
            Dispose();
        }

        public void Dispose()
        {
            if(_tokenWriter != null && _tokenWriter is IDisposable)
            {
                ((IDisposable)_tokenWriter).Dispose();
            }

            if (_tokenizer != null)
            {
                _tokenizer.Dispose();
                _tokenizer = null;
            }
        }

        //
        // Methods
        //

        public void CompileFile()
        {
            if(_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                CompileClass();
                //Console.WriteLine(_tokenizer.CurrentToken.Value);
            }
        }


        //
        // class : 'class' className '{' classVarDec* subroutineDec* '}'
        public void CompileClass()
        {
            if (_tokenizer.CurrentToken.TokenType == TokenType.KEYWORD && _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.CLASS)
            {
                _tokenWriter.WriteTokenStart(_tokenizer.CurrentToken);
                if (_tokenizer.HasMoreTokens())
                {
                    _tokenizer.Advance();
                    var currentToken = _tokenizer.CurrentToken;
                    if(currentToken.TokenType == TokenType.IDENTIFIER)
                    {
                        _tokenWriter.WriteTerminalToken(currentToken);
                    }
                    else
                    {
                        throw new Exception("Bad Syntax");
                    }
                }
                else
                {
                    throw new Exception("Bad Syntax");
                }
            }
        }

        //
        // statements : statement* --> statement : letStatement | ifStatement | whileStatement| doStatement | returnStatement
        public void CompileStatements()
        {

        }

        //
        // ifStatement : 'if' '(' expression ')' '{' statements '}' ('else' '{' statements '}' )?
        public void CompileIfStatement()
        {

        }

        public void CompileWhileStatement()
        {
            _tokenWriter.WriteTokenStart(_tokenizer.CurrentToken);

            //_tokenWriter.WriteTokenEnd<>();
        }

        public void CompileClassVarDec()
        {

        }

        public void CompileSubroutineDec()
        {

        }

        public void CompileParameterList()
        {

        }

        public void CompileSubroutineBody()
        {

        }

        public void CompileVarDec()
        {

        }

        public void CompileLet()
        {

        }

        public void CompileIf()
        {

        }

        public void CompileWhile()
        {

        }

        public void CompileDo()
        {

        }

        public void CompileReturn()
        {

        }

        //
        // Compiles an expression
        public void CompileExpression()
        {

        }

        //
        // Compiles a term
        public void CompileTerm()
        {

        }

        //
        // Compiles a (possibly empty) comma-saparated list of expressions
        public void CompileExpressionList()
        {

        }


        
    }
}
