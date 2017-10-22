using JackCompiler.Contracts;
using JackCompiler.Exceptions;
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
        Tokenizer _tokenizer;
        ITokenWriter _tokenWriter;

        //
        // Ctors / Dtors
        //
        public CompilationEngine(Tokenizer tokenizer, ITokenWriter tokenWriter)
        {
            _tokenizer = tokenizer;
            _tokenWriter = tokenWriter;
        }

        ~CompilationEngine()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_tokenWriter != null && _tokenWriter is IDisposable)
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
            _tokenizer.Advance();
            CompileClass();
        }


        //
        // class : 'class' className '{' classVarDec* subroutineDec* '}'
        public void CompileClass()
        {
            string compUnit = "class";
            if (_tokenizer.CurrentToken.TokenType == TokenType.KEYWORD && _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.CLASS)
            {
                _tokenWriter.WriteTokenStart(compUnit);
                _tokenizer.Advance();

                var currentToken = _tokenizer.CurrentToken;
                if (currentToken.TokenType == TokenType.IDENTIFIER)
                {
                    _tokenWriter.WriteTerminalToken(compUnit, currentToken);
                }
                else
                {
                    throw new BadSyntaxException();
                }
            }
            else
            {
                throw new BadSyntaxException();
            }
        }

        //
        // statements : statement* --> statement : letStatement | ifStatement | whileStatement| doStatement | returnStatement
        public void CompileStatements()
        {
            string compUnit = "statements";

        }

        //
        // ifStatement : 'if' '(' expression ')' '{' statements '}' ('else' '{' statements '}' )?
        public void CompileIfStatement()
        {
            string compUnit = "ifStatement";
        }

        public void CompileLetStatement()
        {
            string compUnit = "letStatement";
        }

        public void CompileWhileStatement()
        {
            string compUnit = "whileStatement";

            if (_tokenizer.CurrentToken.TokenType == TokenType.KEYWORD && _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.WHILE)
            {
                _tokenWriter.WriteTokenStart(compUnit);
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken.Value == "(")
                {
                    _tokenWriter.WriteTerminalToken(compUnit, _tokenizer.CurrentToken);

                }
                else
                {
                    throw new BadSyntaxException();
                }

            }
            else
            {
                throw new BadSyntaxException();
            }

            //_tokenWriter.WriteTokenEnd<>();
        }

        public void CompileClassVarDec()
        {
            string compUnit = "varDec";
        }

        public void CompileSubroutineDec()
        {
            string compUnit = "subroutineDec";
        }

        public void CompileParameterList()
        {
            string compUnit = "parameterList";
        }

        public void CompileSubroutineBody()
        {
            string compUnit = "subroutineBody";
        }

        public void CompileVarDec()
        {
            string compUnit = "varDec";
        }

        public void CompileLet()
        {
            string compUnit = "let";
        }

        public void CompileIf()
        {
            string compUnit = "if";
        }

        public void CompileWhile()
        {
            string compUnit = "while";
        }

        public void CompileDo()
        {
            string compUnit = "do";
        }

        public void CompileReturn()
        {
            string compUnit = "return";
        }

        //
        // expression : term (op term)?
        public void CompileExpression()
        {
            string compUnit = "expression";
        }

        //
        // term : varName | constant
        public void CompileTerm()
        {
            string compUnit = "term";
        }

        //
        // Compiles a (possibly empty) comma-saparated list of expressions
        public void CompileExpressionList()
        {
            string compUnit = "expressionList";
        }



    }
}
