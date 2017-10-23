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
        int depth = 0;

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

        //
        // file: class
        public void CompileFile()
        {
            _tokenizer.Advance();
            CompileClass(0);
        }

        //
        // class: 'class' className '{' classVarDec* subroutineDec* '}'
        public void CompileClass(int depth)
        {
            string compUnit = "class";
            
            // compile 'class'
            var classToken = Eat("class");
            _tokenWriter.WriteTokenStart(compUnit, depth);
            _tokenWriter.WriteTerminalToken(classToken, depth + 1);
            
            // compile className
            var identifierToken = EatIdentifier();
            _tokenWriter.WriteTerminalToken(identifierToken, depth + 1);
            
            // compile '{'
            var leftBraceToken = Eat("{");
            _tokenWriter.WriteTerminalToken(leftBraceToken, depth + 1);
            
            // compile classVarDec*
            while(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.STATIC || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.FIELD)
            {
                CompileClassVarDec(depth + 1);
            }
            
            // compile subroutineDec*
            while(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.METHOD || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.FUNCTION)
            {
                CompileSubroutineDec(depth + 1);
            }
            
            // compile '{'
            var rightBraceToken = Eat("}");
            _tokenWriter.WriteTerminalToken(rightBraceToken, depth + 1);

            // class End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // classVarDec: ('static'|'field') type varName (',' varName)* ';'
        public void CompileClassVarDec(int depth)
        {
            string compUnit = "classVarDec";
            // classVarDec Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: 'static' | 'field'
            var varToken = SoftEat("field") ?? Eat("static");
            _tokenWriter.WriteTerminalToken(varToken, depth);

            // compile: type
            var typeToken = EatType();
            _tokenWriter.WriteTerminalToken(typeToken, depth);

            // compile: varName (',' varName)*
            while(true)
            {
                var varNameToken = EatIdentifier();
                _tokenWriter.WriteTerminalToken(varNameToken, depth);

                if(_tokenizer.CurrentToken.Value == ",")
                {
                    var commaToken = Eat(",");
                    _tokenWriter.WriteTerminalToken(commaToken, depth);
                    continue;
                }
                break;
            }

            // compile: ';'
            var semiColonToken = Eat(";");
            _tokenWriter.WriteTerminalToken(semiColonToken, depth);

            // classVarDec End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // ('constructor'|'function'|'method') ('void'|type) subtroutineName '(' parameterList ')' subroutineBody
        public void CompileSubroutineDec(int depth)
        {
            string compUnit = "subroutineDec";

            // subroutineDec Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: ('constructor'|'function'|'method')
            var subToken = EatSubroutine();
            _tokenWriter.WriteTerminalToken(subToken, depth + 1);

            // compile: ('void'|type)
            var subTypeToken = SoftEatType() ?? Eat("void");
            _tokenWriter.WriteTerminalToken(subTypeToken, depth + 1);

            // compile: subtroutineName
            var subNameToken = EatIdentifier();
            _tokenWriter.WriteTerminalToken(subNameToken, depth + 1);

            // compile: '('
            var leftParenToken = Eat("(");
            _tokenWriter.WriteTerminalToken(leftParenToken, depth + 1);

            // compile: parameterList
            CompileParameterList(depth + 1);

            // compile: ')'
            var rightParenToken = Eat(")");
            _tokenWriter.WriteTerminalToken(rightParenToken, depth + 1);

            // compile: subroutineBody
            CompileSubroutineBody();
        }

        //
        // ( (type varName) (',' type varName)* )?
        public void CompileParameterList(int depth)
        {
            bool commaEncountered = false;
            string compUnit = "parameterList";
            // parameterList Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: ( (type varName) (',' type varName)* )? 
            while(  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.BOOLEAN
                ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.INT
                ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.CHAR
                || commaEncountered)
            {
                // compile: type
                var typeToken = EatType();
                _tokenWriter.WriteTerminalToken(typeToken, depth + 1);

                // compile: varName
                var varNameToken = EatIdentifier();
                _tokenWriter.WriteTerminalToken(varNameToken, depth + 1);

                // compile: ','
                if(_tokenizer.CurrentToken.Value == ",")
                {
                    var commaToken = Eat(",");
                    _tokenWriter.WriteTerminalToken(commaToken, depth + 1);
                    commaEncountered = true;
                    continue;
                }
                else
                {
                    commaEncountered = false;
                }

                break;
            }

            // parameterList End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        public void CompileSubroutineBody()
        {
            string compUnit = "subroutineBody";

            // subroutineBody Start

        }

        //
        // varDec: 'var' type varName (',' varName)* ';'
        public void CompileVarDec()
        {
            string compUnit = "varDec";
        }

        //
        // statements : statement* --> statement : letStatement | ifStatement | whileStatement| doStatement | returnStatement
        public void CompileStatements(int depth)
        {
            string compUnit = "statements";
            while(true)
            { 
                if(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.LET)
                {
                    CompileLetStatement();
                    continue;
                }
                if(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.WHILE)
                {
                    CompileWhileStatement(depth);
                    continue;
                }
                if(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.IF)
                {
                    CompileIfStatement();
                    continue;
                }
                if(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.RETURN)
                {
                    CompileReturnStatement();
                    continue;
                }
                if(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.DO)
                {
                    CompileDoStatement();
                    continue;
                }
                break;
            }

        }

        //
        // ifStatement: 'if' '(' expression ')' '{' statements '}' ('else' '{' statements '}' )?
        public void CompileIfStatement()
        {
            string compUnit = "ifStatement";
        }

        //
        // letStatement: 'let' varName ('[' expression ']')? '=' expression ';'
        public void CompileLetStatement()
        {
            string compUnit = "letStatement";
        }

        //
        // whileStatement: 'while' '(' expression ')' '{' statements '}' ('else' '{' statements '}' )?
        public void CompileWhileStatement(int depth)
        {
            string compUnit = "whileStatement";

            var whileToken = Eat("while");
            _tokenWriter.WriteTokenStart(compUnit, depth);

            var leftParenToken = Eat("(");
            _tokenWriter.WriteTerminalToken(leftParenToken, depth);

            CompileExpression();

            var rightParenToken = Eat(")");
            _tokenWriter.WriteTerminalToken(rightParenToken, depth);

            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // doStatement: 'do' subroutineCall ';'
        public void CompileDoStatement()
        {

        }

        //
        // 'return' expression? ';'
        public void CompileReturnStatement()
        {

        }

        

        //
        // expression : term (op term)?
        public void CompileExpression()
        {
            string compUnit = "expression";
        }

        //
        // term : integerConstant | stringConstant | keywordConstant | varName | varName '[' expression ']' | subroutineCall | '(' expression ')' | unaryOp term
        public void CompileTerm()
        {
            string compUnit = "term";
        }        

        //
        // expressionList: (expression (',' expression)* )?
        public void CompileExpressionList()
        {
            string compUnit = "expressionList";
        }

        //
        // Private helper methods
        //

        private Token SoftEat(string token)
        {
            Token result = null;

            if (_tokenizer.CurrentToken != null && _tokenizer.CurrentToken.Value == token)
            {
                result = _tokenizer.CurrentToken;
                _tokenizer.Advance();
            }

            return result;
        }

        private Token Eat(string token)
        {
            Token result = SoftEat(token);

            if(result == null)
            {
                throw new BadSyntaxException();
            }

            return result;
        }

        private Token EatIdentifier()
        {
            Token result = null;
            if(_tokenizer.CurrentToken != null && _tokenizer.CurrentToken.TokenType == TokenType.IDENTIFIER)
            {
                result = _tokenizer.CurrentToken;
                _tokenizer.Advance();
            }
            else
            {
                throw new BadSyntaxException();
            }
            return result;
        }

        private Token SoftEatType()
        {
            Token result = null;
            if (    _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.INT
                ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.CHAR
                ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.BOOLEAN)
            {
                result = _tokenizer.CurrentToken;
                _tokenizer.Advance();
            }

            return result;
        }

        private Token EatType()
        {
            Token result = SoftEatType();
            if(result == null)
            {
                throw new BadSyntaxException();
            }
            return result;
        }

        private Token EatSubroutine()
        {
            Token result = null;
            if (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.METHOD
                || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.CONSTRUCTOR
                || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.FUNCTION)
            {
                result = _tokenizer.CurrentToken;
                _tokenizer.Advance();
            }
            else
            {
                throw new BadSyntaxException();
            }
            return result;
        }
    }
}
