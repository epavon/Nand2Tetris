﻿using JackCompiler.Contracts;
using JackCompiler.Exceptions;
using JackCompiler.Models;
using JackCompiler.Types;
using JackCompiler.Writer.Contracts;
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
        IVmWriter _vmWriter;

        private readonly char[] ops = { '+', '-', '*', '/', '&','|', '<', '>', '=' };

        //
        // Ctors / Dtors
        //
        
        public CompilationEngine(Tokenizer tokenizer, IVmWriter vmWriter)
        {
            this._tokenizer = tokenizer;
            this._vmWriter = vmWriter;
        }

        ~CompilationEngine()
        {
            Dispose();
        }

        public void Dispose()
        {
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

            // dispose of vmwriter
            if(_vmWriter != null && _vmWriter is IDisposable)
            {
                ((IDisposable)_vmWriter).Dispose();
            }
        }

        //
        // class: 'class' className '{' classVarDec* subroutineDec* '}'
        public void CompileClass(int depth)
        {
            string compUnit = "class";

            // init class symbol table
            SymbolTableManager.ResetClassSymbolTable();
            
            // compile 'class'
            var classToken = Eat("class");
            
            // compile className
            var identifierToken = EatIdentifier();
            
            // compile '{'
            var leftBraceToken = Eat("{");
            
            // compile classVarDec*
            while(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.STATIC || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.FIELD)
            {
                CompileClassVarDec(depth + 1);
            }
            
            // compile subroutineDec*
            while(_tokenizer.CurrentToken != null 
                && (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.METHOD 
                    || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.FUNCTION
                    || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.CONSTRUCTOR))
            {
                CompileSubroutineDec(depth + 1, identifierToken.Value);
            }
            
            // compile '{'
            var rightBraceToken = Eat("}");
        }

        //
        // classVarDec: ('static'|'field') type varName (',' varName)* ';'
        public void CompileClassVarDec(int depth)
        {
            string compUnit = "classVarDec";
			
            // compile: 'static' | 'field'
            var varKind = SoftEat("field") ?? Eat("static");
            var stVarKind = (VarKindType)Enum.Parse(typeof(VarKindType), varKind.Value.ToUpper());

            // compile: type
            var typeToken = EatType();

            // compile: varName (',' varName)*
            while(true)
            {
                var varNameToken = EatIdentifier();
				
                // Add to symbol table
                SymbolTableManager.AddToClassSymbolTable(new SymbolTableItem { Kind = stVarKind, Name = varNameToken.Value, Scope = VarScopeType.CLASS_LEVEL, Type = typeToken.Value });

                if(_tokenizer.CurrentToken.Value == ",")
                {
                    var commaToken = Eat(",");
                    continue;
                }
                break;
            }

            // compile: ';'
            var semiColonToken = Eat(";");

        }

        //
        // ('constructor'|'function'|'method') ('void'|type) subtroutineName '(' parameterList ')' subroutineBody
        public void CompileSubroutineDec(int depth, string className)
        {
            string compUnit = "subroutineDec";
			
            // Reset SymbolTable
            SymbolTableManager.ResetSubroutineSymbolTable();

            // compile: ('constructor'|'function'|'method')
            var subToken = EatSubroutine();
            if (subToken.Value != "function")
            {
                SymbolTableManager.AddToSubroutineSymbolTable(new SymbolTableItem { Name = "this", Type = className, Scope = VarScopeType.SUBROUTINE_LEVEL, Kind = VarKindType.ARGUMENT });
            }

            // compile: ('void'|type)
            var subTypeToken = SoftEatType() ?? Eat("void");

            // compile: subtroutineName
            var subNameToken = EatIdentifier();

            // compile: '('
            var leftParenToken = Eat("(");

            // compile: parameterList
            CompileParameterList(depth + 1);

            // compile: ')'
            var rightParenToken = Eat(")");

            // compile: subroutineBody
            CompileSubroutineBody(depth + 1);

        }

        //
        // ( (type varName) (',' type varName)* )?
        public void CompileParameterList(int depth)
        {
            bool commaEncountered = false;
            string compUnit = "parameterList";

            // compile: ( (type varName) (',' type varName)* )? 
            while(  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.BOOLEAN
                ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.INT
                ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.CHAR
                || commaEncountered)
            {
                // compile: type
                var typeToken = EatType();
                //_tokenWriter.WriteTerminalToken(typeToken, depth + 1);

                // compile: varName
                var varNameToken = EatIdentifier();
                //_tokenWriter.WriteTerminalToken(varNameToken, depth + 1);

                // Add to symbol table
                SymbolTableManager.AddToSubroutineSymbolTable(new SymbolTableItem { Kind = VarKindType.ARGUMENT, Scope = VarScopeType.SUBROUTINE_LEVEL, Type = typeToken.Value, Name = varNameToken.Value });

                // compile: ','
                if(_tokenizer.CurrentToken.Value == ",")
                {
                    var commaToken = Eat(",");
                    commaEncountered = true;
                    continue;
                }
                else
                {
                    commaEncountered = false;
                }

                break;
            }

        }

        //
        // subroutineBody: '{' varDec* statements '}'
        public void CompileSubroutineBody(int depth)
        {
            string compUnit = "subroutineBody";

            // compile: '{'
            var leftBraceToken = Eat("{");

            // compile: varDec*
            while(_tokenizer.CurrentToken.Value == "var")
            {
                CompileVarDec(depth + 1);
            }

            // compile: statements
            CompileStatements(depth + 1);

            // compile: '}'
            var rightBraceToken = Eat("}");

        }

        //
        // varDec: 'var' type varName (',' varName)* ';'
        public void CompileVarDec(int depth)
        {
            string compUnit = "varDec";

            // compile: 'var'
            var varToken = Eat("var");

            // compile: type
            var typeToken = EatType();

            // compile: varName
            var varNameToken = EatIdentifier();

            // Add to symbol table
            SymbolTableManager.AddToSubroutineSymbolTable(new SymbolTableItem { Kind = VarKindType.LOCAL, Name = varNameToken.Value, Scope = VarScopeType.SUBROUTINE_LEVEL, Type = typeToken.Value });

            // compile: (',' varName)*
            while(_tokenizer.CurrentToken.Value == ",")
            {
                // compile: ','
                var commaToken = Eat(",");

                // compile varName
                varNameToken = EatIdentifier();

                // Add to symbol table
                SymbolTableManager.AddToSubroutineSymbolTable(new SymbolTableItem { Kind = VarKindType.LOCAL, Name = varNameToken.Value, Scope = VarScopeType.SUBROUTINE_LEVEL, Type = typeToken.Value });
            }

            // compile: ';'
            var semiColonToken = Eat(";");
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
                    CompileLetStatement(depth + 1);
                    continue;
                }
                if(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.WHILE)
                {
                    CompileWhileStatement(depth + 1);
                    continue;
                }
                if(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.IF)
                {
                    CompileIfStatement(depth + 1);
                    continue;
                }
                if(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.RETURN)
                {
                    CompileReturnStatement(depth + 1);
                    continue;
                }
                if(_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.DO)
                {
                    CompileDoStatement(depth + 1);
                    continue;
                }
                break;
            }

            // statements End
            //_tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // ifStatement: 'if' '(' expression ')' '{' statements '}' ('else' '{' statements '}' )?
        public void CompileIfStatement(int depth)
        {
            string compUnit = "ifStatement";

            // compile: 'if'
            var ifToken = Eat("if");

            // compile: '('
            var leftParenToken = Eat("(");

            // compile: expression
            CompileExpression(depth + 1);

            // compile: ')'
            var rightParenToken = Eat(")");

            // compile: '{'
            var leftBraceToken = Eat("{");

            // compile: statements
            CompileStatements(depth + 1);

            // compile: '}'
            var rightBraceToken = Eat("}");

            // compile: ('else' '{' statements '}' )?
            if(_tokenizer.CurrentToken.Value == "else")
            {
                // compile: 'else'
                var elseToken = Eat("else");

                // compile: '{'
                leftBraceToken = Eat("{");

                // compile: statements
                CompileStatements(depth + 1);

                // compile: '}'
                rightBraceToken = Eat("}");

            }

        }

        //
        // letStatement: 'let' varName ('[' expression ']')? '=' expression ';'
        public void CompileLetStatement(int depth)
        {
            string compUnit = "letStatement";

            // compile: 'let'
            var letToken = Eat("let");

            // compile: varName
            var varNameToken = EatIdentifier();
            var vasAssignedSymbolTableItem = SymbolTableManager.Find(varNameToken.Value);

            // compile: ('[' expression ']')?
            if(_tokenizer.CurrentToken.Value == "[")
            {
                // compile: '['
                var leftBracketToken = Eat("[");

                // compile: expression
                CompileExpression(depth+1);

                // compile: ']'
                var rightBracketToken = Eat("]");
            }

            // compile: '='
            var assignmentToken = Eat("=");

            // compile: expression
            CompileExpression(depth + 1);

            // pop varAssigned


            // compile: ';'
            var semiColonToken = Eat(";");

        }

        //
        // whileStatement: 'while' '(' expression ')' '{' statements '}' 
        public void CompileWhileStatement(int depth)
        {
            string compUnit = "whileStatement";

            // compile: 'while'
            var whileToken = Eat("while");

            // compile: '('
            var leftParenToken = Eat("(");

            // compile: expression
            CompileExpression(depth + 1);

            // compile: ')'
            var rightParenToken = Eat(")");

            // compile: '{'
            var leftBraceToken = Eat("{");

            // compile: statements
            CompileStatements(depth + 1);

            // compile: '}'
            var rightBraceToken = Eat("}");

        }

        //
        // doStatement: 'do' subroutineCall ';'
        public void CompileDoStatement(int depth)
        {
            string compUnit = "doStatement";

            // compile: 'do'
            var doToken = Eat("do");

            // compile: subroutineCall
            CompileSubroutineCall(depth + 1);

            // compile: ';'
            var semiColonToken = Eat(";");

        }

        //
        // 'return' expression? ';'
        public void CompileReturnStatement(int depth)
        {
            string compUnit = "returnStatement";

            // compile: 'return'
            var returnToken = Eat("return");

            // compile: expression?
            if(_tokenizer.CurrentToken.Value != ";")
            {
                CompileExpression(depth + 1);
            }

            // compile: ';'
            var semiColonToken = Eat(";");

        }


        //
        // expression : term (op term)?
        public void CompileExpression(int depth) 
        {
            // compile: term -> push term
            CompileTerm(depth + 1);

            // compile: (op term)?
            if(ops.Contains(_tokenizer.CurrentToken.Value[0]))
            {
                // compile: op
                var opToken = EatOp();

                // compile: term
                CompileTerm(depth + 1);

                // write op
                _vmWriter.WriteOp(opToken);
            }

        }

        //
        // term : integerConstant | stringConstant | keywordConstant | unaryOp term | '(' expression ')' | varName | varName '[' expression ']' | subroutineCall 
        public void CompileTerm(int depth)
        {
            string compUnit = "term";
            CompilationUnit result = new CompilationUnit { Name = compUnit, CompUnits = new List<Token>() };

            // compile: integerConstant
            if(_tokenizer.CurrentToken.TokenType == TokenType.INT_COSNT)
            {
                _vmWriter.WritePush("constant", Convert.ToInt32(_tokenizer.CurrentToken.Value));
                _tokenizer.Advance();
            }
            // compile: stringConstant
            else if(_tokenizer.CurrentToken.TokenType == TokenType.STRING_CONST)
            {
                _tokenizer.Advance();
            }
            // compile: keywordConstant
            else if(    _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.TRUE 
                    ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.FALSE
                    ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.NULL
                    ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.THIS)
            {
                
                _tokenizer.Advance();
            }
            // compile: unaryOp term
            else if(_tokenizer.CurrentToken.Value == "~" || _tokenizer.CurrentToken.Value == "-")
            {
                // compile: unaryOp
                var unaryOpToken = _tokenizer.CurrentToken;
                _tokenizer.Advance();

                // compile: term
                CompileTerm(depth + 1);

                // write op
                _vmWriter.WriteOp(unaryOpToken);
            }
            // compile: '(' expression ')'
            else if(_tokenizer.CurrentToken.Value == "(")
            {
                // compile '('
                var leftParenToken = Eat("(");

                // compile: expression
                CompileExpression(depth + 1);

                // compile: ')'
                var rightParenToken = Eat(")");
            }
            // compile: varName | varName '[' expression ']' | subroutineCall '(' expression ')'
            else if(_tokenizer.CurrentToken.TokenType == TokenType.IDENTIFIER)
            {
                var nextToken = _tokenizer.Peek();
                var sbVarName = SymbolTableManager.Find(_tokenizer.CurrentToken.Value);

                // compile: varName '[' expression ']'
                if(nextToken.Value == "[")
                {
                    // compile: varName
                    var varNameToken = EatIdentifier();
                    
                    result.CompUnits.Add(varNameToken);

                    // compile: '['
                    var leftBracketToken = Eat("[");

                    // compile: expression
                    CompileExpression(depth + 1);

                    // compile: ']'
                    var rightBracketToken = Eat("]");
                }
                // compile: subroutineCall
                else if(nextToken.Value == "(" || nextToken.Value == ".")
                {
                    CompileSubroutineCall(depth + 1);
                    
                }
                // compile: varName
                else
                {
                    var varNameToken = EatIdentifier();
                    _vmWriter.WritePush(sbVarName.Kind.ToString(), sbVarName.Number);
                }
            }

        }        

        //
        // subroutineCall: subroutineName '(' expressionList ')' | (className | varName) '.' subroutineName '(' expressionList '}'
        public void CompileSubroutineCall(int depth)
        {
            var sbSubName = SymbolTableManager.Find(_tokenizer.CurrentToken.Value);
            var nextToken = _tokenizer.Peek();
            // compile: subroutineName '(' expressionList ')'
            if (nextToken.Value == "(")
            {
                // compile: subroutineName
                var subName = EatIdentifier();

                // compile: '('
                var leftParenToken = Eat("(");

                // compile: expressionList
                CompileExpressionList(depth);

                // compile: ')'
                var rightParenToken = Eat(")");

                // write call
                _vmWriter.WriteCall(sbSubName.Name, 0);
            }
            else if (nextToken.Value == ".")
            {
                // compile: (className | varName)
                var nameToken = EatIdentifier();

                // compile '.'
                var dotToken = Eat(".");

                // compile: subroutineName
                var subNameToken = EatIdentifier();

                // modify sub name
                

                // compile: '('
                var leftParenToken = Eat("(");

                // compile: expression
                CompileExpressionList(depth);

                // compile: ')'
                var rightParenToken = Eat(")");
            }
        }

        //
        // expressionList: (expression (',' expression)* )?
        public void CompileExpressionList(int depth)
        {
            string compUnit = "expressionList";

            // expressionList Start
            //_tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: (expression (',' expression)* )?
            if(_tokenizer.CurrentToken.Value != ")")
            {
                CompileExpression(depth + 1);
                while(_tokenizer.CurrentToken.Value == ",")
                {
                    // compile: ','
                    var commaToken = Eat(",");
                    //_tokenWriter.WriteTerminalToken(commaToken, depth + 1);

                    // compile: expression
                    CompileExpression(depth + 1);
                }
            }

            // expressionList End
            //_tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        /////////////////////////////////////////////
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

        private Token SoftEatIdentifier()
        {
            Token result = null;
            if (_tokenizer.CurrentToken != null && _tokenizer.CurrentToken.TokenType == TokenType.IDENTIFIER)
            {
                result = _tokenizer.CurrentToken;
                _tokenizer.Advance();
            }
            return result;
        }

        private Token EatIdentifier()
        {
            var result = SoftEatIdentifier();
            if(result == null)
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

            // handle type being class name
            if (result == null)
            {
                result = SoftEatIdentifier();
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

        private Token SoftEatOp()
        {
            Token result = null;
            if(ops.Contains(_tokenizer.CurrentToken.Value[0]))
            {
                result = _tokenizer.CurrentToken;
                _tokenizer.Advance();
            }

            return result;
        }

        private Token EatOp()
        {
            Token result = SoftEatOp();
            if (result == null)
            {
                throw new BadSyntaxException();
            }
            return result;
        }
    }
}
