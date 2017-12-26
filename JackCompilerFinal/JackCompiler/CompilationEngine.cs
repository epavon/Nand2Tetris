using JackCompiler.Contracts;
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
        ITokenWriter _tokenWriter;
        IVmWriter _codeWriter;

        private readonly char[] ops = { '+', '-', '*', '/', '&','|', '<', '>', '=' };

        //
        // Ctors / Dtors
        //
        public CompilationEngine(Tokenizer tokenizer, ITokenWriter tokenWriter)
        {
            _tokenizer = tokenizer;
            _tokenWriter = tokenWriter;
        }

        public CompilationEngine(Tokenizer tokenizer, IVmWriter codeWriter)
        {
            this._tokenizer = tokenizer;
            this._codeWriter = codeWriter;
        }

        public CompilationEngine(Tokenizer tokenizer, ITokenWriter tokenWriter, IVmWriter codeWriter)
        {
            this._tokenizer = tokenizer;
            this._tokenWriter = tokenWriter;
            this._codeWriter = codeWriter;
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

            // dispose of writer
            if (_tokenWriter != null && _tokenWriter is IDisposable)
            {
                ((IDisposable)_tokenWriter).Dispose();
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
            while(_tokenizer.CurrentToken != null 
                && (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.METHOD 
                    || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.FUNCTION
                    || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.CONSTRUCTOR))
            {
                CompileSubroutineDec(depth + 1, identifierToken.Value);
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
            var varKind = SoftEat("field") ?? Eat("static");
			_tokenWriter.WriteTerminalToken(varKind, depth + 1);
            var stVarKind = (VarKindType)Enum.Parse(typeof(VarKindType), varKind.Value.ToUpper());

            // compile: type
            var typeToken = EatType();
            _tokenWriter.WriteTerminalToken(typeToken, depth + 1);

            // compile: varName (',' varName)*
            while(true)
            {
                var varNameToken = EatIdentifier();
				_tokenWriter.WriteTerminalToken(varNameToken, depth + 1);
				
                // Add to symbol table
                SymbolTableManager.AddToClassSymbolTable(new SymbolTableItem { Kind = stVarKind, Name = varNameToken.Value, Scope = VarScopeType.CLASS_LEVEL, Type = typeToken.Value });

                if(_tokenizer.CurrentToken.Value == ",")
                {
                    var commaToken = Eat(",");
					_tokenWriter.WriteTerminalToken(commaToken, depth + 1);
                    continue;
                }
                break;
            }

            // compile: ';'
            var semiColonToken = Eat(";");
            _tokenWriter.WriteTerminalToken(semiColonToken, depth + 1);

            // classVarDec End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // ('constructor'|'function'|'method') ('void'|type) subtroutineName '(' parameterList ')' subroutineBody
        public void CompileSubroutineDec(int depth, string className)
        {
            string compUnit = "subroutineDec";
			
			// subroutineDec Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // Reset SymbolTable
            SymbolTableManager.ResetSubroutineSymbolTable();

            // compile: ('constructor'|'function'|'method')
            var subToken = EatSubroutine();
            _tokenWriter.WriteTerminalToken(subToken, depth + 1);
            if (subToken.Value != "function")
            {
                SymbolTableManager.AddToSubroutineSymbolTable(new SymbolTableItem { Name = "this", Type = className, Scope = VarScopeType.SUBROUTINE_LEVEL, Kind = VarKindType.ARGUMENT });
            }

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
            CompileSubroutineBody(depth + 1);

            // subroutineDec End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
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

                // Add to symbol table
                SymbolTableManager.AddToSubroutineSymbolTable(new SymbolTableItem { Kind = VarKindType.ARGUMENT, Scope = VarScopeType.SUBROUTINE_LEVEL, Type = typeToken.Value, Name = varNameToken.Value });

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

        //
        // subroutineBody: '{' varDec* statements '}'
        public void CompileSubroutineBody(int depth)
        {
            string compUnit = "subroutineBody";

            // subroutineBody Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: '{'
            var leftBraceToken = Eat("{");
            _tokenWriter.WriteTerminalToken(leftBraceToken, depth + 1);

            // compile: varDec*
            while(_tokenizer.CurrentToken.Value == "var")
            {
                CompileVarDec(depth + 1);
            }

            // compile: statements
            CompileStatements(depth + 1);

            // compile: '}'
            var rightBraceToken = Eat("}");
            _tokenWriter.WriteTerminalToken(rightBraceToken, depth + 1);

            // subroutineBody End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // varDec: 'var' type varName (',' varName)* ';'
        public void CompileVarDec(int depth)
        {
            string compUnit = "varDec";

            // varDec Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: 'var'
            var varToken = Eat("var");
            _tokenWriter.WriteTerminalToken(varToken, depth+1);

            // compile: type
            var typeToken = EatType();
            _tokenWriter.WriteTerminalToken(typeToken, depth + 1);

            // compile: varName
            var varNameToken = EatIdentifier();
            _tokenWriter.WriteTerminalToken(varNameToken, depth + 1);

            // Add to symbol table
            SymbolTableManager.AddToSubroutineSymbolTable(new SymbolTableItem { Kind = VarKindType.LOCAL, Name = varNameToken.Value, Scope = VarScopeType.SUBROUTINE_LEVEL, Type = typeToken.Value });

            // compile: (',' varName)*
            while(_tokenizer.CurrentToken.Value == ",")
            {
                // compile: ','
                var commaToken = Eat(",");
                _tokenWriter.WriteTerminalToken(commaToken, depth + 1);

                // compile varName
                varNameToken = EatIdentifier();
                _tokenWriter.WriteTerminalToken(varNameToken, depth + 1);

                // Add to symbol table
                SymbolTableManager.AddToSubroutineSymbolTable(new SymbolTableItem { Kind = VarKindType.LOCAL, Name = varNameToken.Value, Scope = VarScopeType.SUBROUTINE_LEVEL, Type = typeToken.Value });
            }

            // compile: ';'
            var semiColonToken = Eat(";");
            _tokenWriter.WriteTerminalToken(semiColonToken, depth + 1);

            // varDec End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // statements : statement* --> statement : letStatement | ifStatement | whileStatement| doStatement | returnStatement
        public void CompileStatements(int depth)
        {
            string compUnit = "statements";

            // statements Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

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
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // ifStatement: 'if' '(' expression ')' '{' statements '}' ('else' '{' statements '}' )?
        public void CompileIfStatement(int depth)
        {
            string compUnit = "ifStatement";

            // ifStatement Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: 'if'
            var ifToken = Eat("if");
            _tokenWriter.WriteTerminalToken(ifToken, depth + 1);

            // compile: '('
            var leftParenToken = Eat("(");
            _tokenWriter.WriteTerminalToken(leftParenToken, depth + 1);

            // compile: expression
            CompileExpression(depth + 1);

            // compile: ')'
            var rightParenToken = Eat(")");
            _tokenWriter.WriteTerminalToken(rightParenToken, depth + 1);

            // compile: '{'
            var leftBraceToken = Eat("{");
            _tokenWriter.WriteTerminalToken(leftBraceToken, depth + 1);

            // compile: statements
            CompileStatements(depth + 1);

            // compile: '}'
            var rightBraceToken = Eat("}");
            _tokenWriter.WriteTerminalToken(rightBraceToken, depth + 1);

            // compile: ('else' '{' statements '}' )?
            if(_tokenizer.CurrentToken.Value == "else")
            {
                // compile: 'else'
                var elseToken = Eat("else");
                _tokenWriter.WriteTerminalToken(elseToken, depth + 1);

                // compile: '{'
                leftBraceToken = Eat("{");
                _tokenWriter.WriteTerminalToken(leftBraceToken, depth + 1); 

                // compile: statements
                CompileStatements(depth + 1);

                // compile: '}'
                rightBraceToken = Eat("}");
                _tokenWriter.WriteTerminalToken(rightBraceToken, depth + 1);

            }

            // ifStatement End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // letStatement: 'let' varName ('[' expression ']')? '=' expression ';'
        public void CompileLetStatement(int depth)
        {
            string compUnit = "letStatement";

            // letStatement Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: 'let'
            var letToken = Eat("let");
            _tokenWriter.WriteTerminalToken(letToken, depth + 1);

            // compile: varName
            var varNameToken = EatIdentifier();
            _tokenWriter.WriteTerminalToken(varNameToken, depth + 1);
            var vasAssignedSymbolTableItem = SymbolTableManager.Find(varNameToken.Value);

            // compile: ('[' expression ']')?
            if(_tokenizer.CurrentToken.Value == "[")
            {
                // compile: '['
                var leftBracketToken = Eat("[");
                _tokenWriter.WriteTerminalToken(leftBracketToken, depth+1);

                // compile: expression
                CompileExpression(depth+1);

                // compile: ']'
                var rightBracketToken = Eat("]");
                _tokenWriter.WriteTerminalToken(rightBracketToken, depth + 1);
            }

            // compile: '='
            var assignmentToken = Eat("=");
            _tokenWriter.WriteTerminalToken(assignmentToken, depth + 1);

            // compile: expression
            CompileExpression(depth + 1);

            // pop varAssigned


            // compile: ';'
            var semiColonToken = Eat(";");
            _tokenWriter.WriteTerminalToken(semiColonToken, depth + 1);

            // letStatement End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // whileStatement: 'while' '(' expression ')' '{' statements '}' 
        public void CompileWhileStatement(int depth)
        {
            string compUnit = "whileStatement";

            // whileStatement Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: 'while'
            var whileToken = Eat("while");
            _tokenWriter.WriteTerminalToken(whileToken, depth + 1);

            // compile: '('
            var leftParenToken = Eat("(");
            _tokenWriter.WriteTerminalToken(leftParenToken, depth + 1);

            // compile: expression
            CompileExpression(depth + 1);

            // compile: ')'
            var rightParenToken = Eat(")");
            _tokenWriter.WriteTerminalToken(rightParenToken, depth + 1);

            // compile: '{'
            var leftBraceToken = Eat("{");
            _tokenWriter.WriteTerminalToken(leftBraceToken, depth + 1);

            // compile: statements
            CompileStatements(depth + 1);

            // compile: '}'
            var rightBraceToken = Eat("}");
            _tokenWriter.WriteTerminalToken(rightBraceToken, depth + 1);

            // whileStatement End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // doStatement: 'do' subroutineCall ';'
        public void CompileDoStatement(int depth)
        {
            string compUnit = "doStatement";

            // doStatement Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: 'do'
            var doToken = Eat("do");
            _tokenWriter.WriteTerminalToken(doToken, depth + 1);

            // compile: subroutineCall
            CompileSubroutineCall(depth + 1);

            // compile: ';'
            var semiColonToken = Eat(";");
            _tokenWriter.WriteTerminalToken(semiColonToken, depth + 1);

            // doStatement End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // 'return' expression? ';'
        public void CompileReturnStatement(int depth)
        {
            string compUnit = "returnStatement";

            // returnStatement Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: 'return'
            var returnToken = Eat("return");
            _tokenWriter.WriteTerminalToken(returnToken, depth + 1);

            // compile: expression?
            if(_tokenizer.CurrentToken.Value != ";")
            {
                CompileExpression(depth + 1);
            }

            // compile: ';'
            var semiColonToken = Eat(";");
            _tokenWriter.WriteTerminalToken(semiColonToken, depth + 1);

            // returnStatement End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        

        //
        // expression : term (op term)?
        public void CompileExpression(int depth) //@TODO: Build an expression object and then write it using the algorithm..
        {
            string compUnit = "expression";

            // expression Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: term -> push term
            var termCU = CompileTerm(depth + 1);



            // compile: (op term)?
            if(ops.Contains(_tokenizer.CurrentToken.Value[0]))
            {
                // compile: op
                var opToken = EatOp();
                _tokenWriter.WriteTerminalToken(opToken, depth + 1);

                // compile: term
                CompileTerm(depth + 1);
            }

            // expression End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
        }

        //
        // term : integerConstant | stringConstant | keywordConstant | unaryOp term | '(' expression ')' | varName | varName '[' expression ']' | subroutineCall 
        public CompilationUnit CompileTerm(int depth)
        {
            string compUnit = "term";
            CompilationUnit result = new CompilationUnit { Name = compUnit, CompUnits = new List<Token>() };

            // term Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: integerConstant
            if(_tokenizer.CurrentToken.TokenType == TokenType.INT_COSNT)
            {
                result.CompUnits.Add(_tokenizer.CurrentToken);
                _tokenWriter.WriteTerminalToken(_tokenizer.CurrentToken, depth + 1);
                _tokenizer.Advance();
            }
            // compile: stringConstant
            else if(_tokenizer.CurrentToken.TokenType == TokenType.STRING_CONST)
            {
                result.CompUnits.Add(_tokenizer.CurrentToken);
                _tokenWriter.WriteTerminalToken(_tokenizer.CurrentToken, depth + 1);
                _tokenizer.Advance();
            }
            // compile: keywordConstant
            else if(    _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.TRUE 
                    ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.FALSE
                    ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.NULL
                    ||  _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.THIS)
            {
                
                _tokenWriter.WriteTerminalToken(_tokenizer.CurrentToken, depth + 1);
                _tokenizer.Advance();
            }
            // compile: unaryOp term
            else if(_tokenizer.CurrentToken.Value == "~" || _tokenizer.CurrentToken.Value == "-")
            {
                // compile: unaryOp
                result.CompUnits.Add(_tokenizer.CurrentToken);
                _tokenWriter.WriteTerminalToken(_tokenizer.CurrentToken, depth + 1);
                _tokenizer.Advance();
                // compile: term
                CompileTerm(depth + 1);
            }
            // compile: '(' expression ')'
            else if(_tokenizer.CurrentToken.Value == "(")
            {
                // compile '('
                var leftParenToken = Eat("(");
                _tokenWriter.WriteTerminalToken(leftParenToken, depth + 1);

                // compile: expression
                CompileExpression(depth + 1);

                // compile: ')'
                var rightParenToken = Eat(")");
                _tokenWriter.WriteTerminalToken(rightParenToken, depth + 1);

            }
            // compile: varName | varName '[' expression ']' | subroutineCall '(' expression ')'
            else if(_tokenizer.CurrentToken.TokenType == TokenType.IDENTIFIER)
            {
                var nextToken = _tokenizer.Peek();
                // compile: varName '[' expression ']'
                if(nextToken.Value == "[")
                {
                    // compile: varName
                    var varNameToken = EatIdentifier();
                    result.CompUnits.Add(varNameToken);
                    _tokenWriter.WriteTerminalToken(varNameToken, depth + 1);

                    // compile: '['
                    var leftBracketToken = Eat("[");
                    _tokenWriter.WriteTerminalToken(leftBracketToken, depth + 1);

                    // compile: expression
                    CompileExpression(depth + 1);

                    // compile: ']'
                    var rightBracketToken = Eat("]");
                    _tokenWriter.WriteTerminalToken(rightBracketToken, depth + 1);
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
                    _tokenWriter.WriteTerminalToken(varNameToken, depth + 1);
                }
            }

            // term End
            _tokenWriter.WriteTokenEnd(compUnit, depth);

            return result;
        }        

        //
        // subroutineCall: subroutineName '(' expressionList ')' | (className | varName) '.' subroutineName '(' expressionList '}'
        public void CompileSubroutineCall(int depth)
        {
            var nextToken = _tokenizer.Peek();
            // compile: subroutineName '(' expressionList ')'
            if (nextToken.Value == "(")
            {
                // compile: subroutineName
                var subName = EatIdentifier();
                _tokenWriter.WriteTerminalToken(subName, depth);

                // compile: '('
                var leftParenToken = Eat("(");
                _tokenWriter.WriteTerminalToken(leftParenToken, depth);

                // compile: expressionList
                CompileExpressionList(depth);

                // compile: ')'
                var rightParenToken = Eat(")");
                _tokenWriter.WriteTerminalToken(rightParenToken, depth);
            }
            else if (nextToken.Value == ".")
            {
                // compile: (className | varName)
                var nameToken = EatIdentifier();
                _tokenWriter.WriteTerminalToken(nameToken, depth);

                // compile '.'
                var dotToken = Eat(".");
                _tokenWriter.WriteTerminalToken(dotToken, depth);

                // compile: subroutineName
                var subNameToken = EatIdentifier();
                _tokenWriter.WriteTerminalToken(subNameToken, depth);

                // compile: '('
                var leftParenToken = Eat("(");
                _tokenWriter.WriteTerminalToken(leftParenToken, depth);

                // compile: expression
                CompileExpressionList(depth);

                // compile: ')'
                var rightParenToken = Eat(")");
                _tokenWriter.WriteTerminalToken(rightParenToken, depth);
            }
        }

        //
        // expressionList: (expression (',' expression)* )?
        public void CompileExpressionList(int depth)
        {
            string compUnit = "expressionList";

            // expressionList Start
            _tokenWriter.WriteTokenStart(compUnit, depth);

            // compile: (expression (',' expression)* )?
            if(_tokenizer.CurrentToken.Value != ")")
            {
                CompileExpression(depth + 1);
                while(_tokenizer.CurrentToken.Value == ",")
                {
                    // compile: ','
                    var commaToken = Eat(",");
                    _tokenWriter.WriteTerminalToken(commaToken, depth + 1);

                    // compile: expression
                    CompileExpression(depth + 1);
                }
            }

            // expressionList End
            _tokenWriter.WriteTokenEnd(compUnit, depth);
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
