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
        IVmWriter _vmWriter;
        string className = string.Empty;
        int _whilelabelCounter = 0;
        int _ifLabelCounter = 0;

        public int WhileLabelCounter
        {
            get { _whilelabelCounter++; return _whilelabelCounter - 1; }
        }

        public int IfLabelCounter
        {
            get
            {
                _ifLabelCounter++; return _ifLabelCounter - 1;
            }
        }

        private readonly char[] ops = { '+', '-', '*', '/', '&', '|', '<', '>', '=' };

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
            if (_vmWriter != null && _vmWriter is IDisposable)
            {
                ((IDisposable)_vmWriter).Dispose();
            }
        }

        //
        // class: 'class' className '{' classVarDec* subroutineDec* '}'
        public void CompileClass(int depth)
        {
            // init class symbol table
            SymbolTableManager.ResetClassSymbolTable();

            // compile 'class'
            var classToken = Eat("class");

            // compile className
            var identifierToken = EatIdentifier();
            className = identifierToken.Value;

            // compile '{'
            var leftBraceToken = Eat("{");

            // compile classVarDec*
            while (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.STATIC || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.FIELD)
            {
                CompileClassVarDec(depth + 1);
            }

            // compile subroutineDec*
            while (_tokenizer.CurrentToken != null
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
            while (true)
            {
                var varNameToken = EatIdentifier();

                // Add to symbol table
                SymbolTableManager.AddToClassSymbolTable(new SymbolTableItem { Kind = stVarKind, Name = varNameToken.Value, Scope = VarScopeType.CLASS_LEVEL, Type = typeToken.Value });

                if (_tokenizer.CurrentToken.Value == ",")
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
            if (subToken.Value != "function" && subToken.Value != "constructor")
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
            CompileSubroutineBody(depth + 1, subToken.Value, subNameToken.Value, className);

        }

        //
        // ( (type varName) (',' type varName)* )?
        public void CompileParameterList(int depth)
        {
            bool commaEncountered = false;
            string compUnit = "parameterList";

            // compile: ( (type varName) (',' type varName)* )? 
            while (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.BOOLEAN
                || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.INT
                || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.CHAR
                || commaEncountered)
            {
                // compile: type
                var typeToken = EatType();

                // compile: varName
                var varNameToken = EatIdentifier();

                // Add to symbol table
                SymbolTableManager.AddToSubroutineSymbolTable(new SymbolTableItem { Kind = VarKindType.ARGUMENT, Scope = VarScopeType.SUBROUTINE_LEVEL, Type = typeToken.Value, Name = varNameToken.Value });

                // compile: ','
                if (_tokenizer.CurrentToken.Value == ",")
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
        public void CompileSubroutineBody(int depth, string subType, string subName, string className)
        {
            // compile: '{'
            var leftBraceToken = Eat("{");

            // compile: varDec*
            while (_tokenizer.CurrentToken.Value == "var")
            {
                CompileVarDec(depth + 1);
            }
            _vmWriter.WriteFunction(string.Format("{0}.{1}", className, subName), SymbolTableManager.GetLocalsNum());
            // if constructor -> allocate
            if (subType == "constructor")
            {
                int wordsToAllocate = SymbolTableManager.GetFieldsNum();
                _vmWriter.WritePush("constant", wordsToAllocate);
                _vmWriter.WriteCall("Memory.alloc", 1);
                _vmWriter.WritePop("pointer", 0);
            } 
            else if(subType == "method")
            {
                _vmWriter.WritePush("argument", 0);
                _vmWriter.WritePop("pointer", 0);
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
            // compile: 'var'
            var varToken = Eat("var");

            // compile: type
            var typeToken = EatType();

            // compile: varName
            var varNameToken = EatIdentifier();

            // Add to symbol table
            SymbolTableManager.AddToSubroutineSymbolTable(new SymbolTableItem { Kind = VarKindType.LOCAL, Name = varNameToken.Value, Scope = VarScopeType.SUBROUTINE_LEVEL, Type = typeToken.Value });

            // compile: (',' varName)*
            while (_tokenizer.CurrentToken.Value == ",")
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

            while (true)
            {
                if (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.LET)
                {
                    CompileLetStatement(depth + 1);
                    continue;
                }
                if (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.WHILE)
                {
                    CompileWhileStatement(depth + 1);
                    continue;
                }
                if (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.IF)
                {
                    CompileIfStatement(depth + 1);
                    continue;
                }
                if (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.RETURN)
                {
                    CompileReturnStatement(depth + 1);
                    continue;
                }
                if (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.DO)
                {
                    CompileDoStatement(depth + 1);
                    continue;
                }
                break;
            }

        }

        //
        // ifStatement: 'if' '(' expression ')' '{' statements '}' ('else' '{' statements '}' )?
        public void CompileIfStatement(int depth)
        {
            string startLabel = "LIF_" + IfLabelCounter;
            string endLabel = "LIF_" + IfLabelCounter;

            // compile: 'if'
            var ifToken = Eat("if");

            // compile: '('
            var leftParenToken = Eat("(");

            // compile: expression
            CompileExpression(depth + 1);

            // write: not , if-goto startLabel
            _vmWriter.WriteUnaryOp(new Token { Value = "~" });
            _vmWriter.WriteIfGoto(startLabel);

            // compile: ')'
            var rightParenToken = Eat(")");

            // compile: '{'
            var leftBraceToken = Eat("{");

            // compile: statements
            CompileStatements(depth + 1);

            // write: goto endLabel
            _vmWriter.WriteGoto(endLabel);

            // write: label startLabel
            _vmWriter.WriteLabel(startLabel);

            // compile: '}'
            var rightBraceToken = Eat("}");

            // compile: ('else' '{' statements '}' )?
            if (_tokenizer.CurrentToken.Value == "else")
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

            // write: endLabel
            _vmWriter.WriteLabel(endLabel);

        }

        //
        // letStatement: 'let' varName ('[' expression ']')? '=' expression ';'
        public void CompileLetStatement(int depth)
        {
            // compile: 'let'
            var letToken = Eat("let");

            // compile: varName
            var varNameToken = EatIdentifier();
            var sbVarName = SymbolTableManager.Find(varNameToken.Value);

            // compile: ('[' expression ']')?
            if (_tokenizer.CurrentToken.Value == "[")
            {
                // compile: '['
                var leftBracketToken = Eat("[");

                // compile: expression
                CompileExpression(depth + 1);

                // compile: ']'
                var rightBracketToken = Eat("]");
            }

            // compile: '='
            var assignmentToken = Eat("=");

            // compile: expression
            CompileExpression(depth + 1);

            // pop varAssigned
            _vmWriter.WritePop(sbVarName.KindDisplay, sbVarName.Number);

            // compile: ';'
            var semiColonToken = Eat(";");

        }

        //
        // whileStatement: 'while' '(' expression ')' '{' statements '}' 
        public void CompileWhileStatement(int depth)
        {
            string compUnit = "whileStatement";

            string startLabel = "LWHILE_" + WhileLabelCounter;
            string endLabel = "LWHILE_" + WhileLabelCounter;

            // compile: 'while'
            var whileToken = Eat("while");
            _vmWriter.WriteLabel(startLabel);

            // compile: '('
            var leftParenToken = Eat("(");

            // compile: expression
            CompileExpression(depth + 1);

            // write: go to end label
            _vmWriter.WriteUnaryOp(new Token { Value = "~" });
            _vmWriter.WriteIfGoto(endLabel);

            // compile: ')'
            var rightParenToken = Eat(")");

            // compile: '{'
            var leftBraceToken = Eat("{");

            // compile: statements
            CompileStatements(depth + 1);

            // write: go to start label & end label
            _vmWriter.WriteGoto(startLabel);
            _vmWriter.WriteLabel(endLabel);

            // compile: '}'
            var rightBraceToken = Eat("}");

        }

        //
        // doStatement: 'do' subroutineCall ';'
        public void CompileDoStatement(int depth)
        {
            // compile: 'do'
            var doToken = Eat("do");

            // compile: subroutineCall
            CompileSubroutineCall(depth + 1);

            // compile: ';'
            var semiColonToken = Eat(";");

            // write pop
            _vmWriter.WritePop("temp", 0);
        }

        //
        // 'return' expression? ';'
        public void CompileReturnStatement(int depth)
        {
            // compile: 'return'
            var returnToken = Eat("return");

            // compile: expression?
            if (_tokenizer.CurrentToken.Value != ";")
            {
                CompileExpression(depth + 1);
            }
            else
            {
                _vmWriter.WritePush("constant", 0);
            }

            // compile: ';'
            var semiColonToken = Eat(";");

            // write return
            _vmWriter.WriteReturn();
        }


        //
        // expression : term (op term)?
        public void CompileExpression(int depth)
        {
            // compile: term -> push term
            CompileTerm(depth + 1);

            // compile: (op term)?
            if (ops.Contains(_tokenizer.CurrentToken.Value[0]))
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

            // compile: integerConstant
            if (_tokenizer.CurrentToken.TokenType == TokenType.INT_CONST)
            {
                _vmWriter.WritePush("constant", Convert.ToInt32(_tokenizer.CurrentToken.Value));
                _tokenizer.Advance();
            }
            // compile: stringConstant
            else if (_tokenizer.CurrentToken.TokenType == TokenType.STRING_CONST)
            {
                int strLen = _tokenizer.CurrentToken.Value.Length;
                _vmWriter.WritePush("constant", strLen);
                _vmWriter.WriteCall("String.new", 1);
                foreach (char strChar in _tokenizer.CurrentToken.Value)
                {
                    _vmWriter.WritePush("constant", (int)strChar);
                    _vmWriter.WriteCall("String.appendChar", 2);
                }
                _tokenizer.Advance();
            }
            // compile: keywordConstant
            else if (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.TRUE
                    || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.FALSE
                    || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.NULL
                    || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.THIS)
            {
                if (_tokenizer.CurrentToken.GetKeywordType() == KeywordType.TRUE)
                {
                    _vmWriter.WritePush("constant", 0);
                    _vmWriter.WriteUnaryOp(new Token { Value = "~" });
                }
                if (_tokenizer.CurrentToken.GetKeywordType() == KeywordType.FALSE)
                {
                    _vmWriter.WritePush("constant", 0);
                }
                if(_tokenizer.CurrentToken.GetKeywordType() == KeywordType.THIS)
                {
                    _vmWriter.WritePush("pointer", 0);
                }
                _tokenizer.Advance();
            }
            // compile: unaryOp term
            else if (_tokenizer.CurrentToken.Value == "~" || _tokenizer.CurrentToken.Value == "-")
            {
                // compile: unaryOp
                var unaryOpToken = _tokenizer.CurrentToken;
                _tokenizer.Advance();

                // compile: term
                CompileTerm(depth + 1);

                // write op
                _vmWriter.WriteUnaryOp(unaryOpToken);
            }
            // compile: '(' expression ')'
            else if (_tokenizer.CurrentToken.Value == "(")
            {
                // compile '('
                var leftParenToken = Eat("(");

                // compile: expression
                CompileExpression(depth + 1);

                // compile: ')'
                var rightParenToken = Eat(")");
            }
            // compile: varName | varName '[' expression ']' | subroutineCall '(' expression ')'
            else if (_tokenizer.CurrentToken.TokenType == TokenType.IDENTIFIER)
            {
                var nextToken = _tokenizer.Peek();
                var sbVarName = SymbolTableManager.Find(_tokenizer.CurrentToken.Value);

                // compile: varName '[' expression ']'
                if (nextToken.Value == "[")
                {
                    // compile: varName
                    var varNameToken = EatIdentifier();
                    _vmWriter.WritePush(sbVarName.KindDisplay, sbVarName.Number);

                    // compile: '['
                    var leftBracketToken = Eat("[");

                    // compile: expression
                    CompileExpression(depth + 1);

                    // add offset
                    _vmWriter.WriteOp(new Token { Value = "+" });
                    _vmWriter.WritePop("pointer", 1);
                    _vmWriter.WritePush("that", 0);

                    // compile: ']'
                    var rightBracketToken = Eat("]");
                }
                // compile: subroutineCall
                else if (nextToken.Value == "(" || nextToken.Value == ".")
                {
                    CompileSubroutineCall(depth + 1);
                }
                // compile: varName
                else
                {
                    var varNameToken = EatIdentifier();
                    _vmWriter.WritePush(sbVarName.KindDisplay, sbVarName.Number);
                }
            }

        }

        //
        // subroutineCall: subroutineName '(' expressionList ')' | (className | varName) '.' subroutineName '(' expressionList '}'
        public void CompileSubroutineCall(int depth)
        {
            int args = 0;
            int argadder = 0;
            string sbSubName = string.Empty;
            var nextToken = _tokenizer.Peek();

            // compile: subroutineName '(' expressionList ')'
            if (nextToken.Value == "(")
            {
                argadder = 1;
                // compile: subroutineName
                var subName = EatIdentifier();
                sbSubName = className + "." + subName.Value;

                // handle: push 'this' to stack -> push pointer 0
                _vmWriter.WritePush("pointer", 0);

                // compile: '('
                var leftParenToken = Eat("(");

                // compile: expressionList
                args = CompileExpressionList(depth) + argadder;

                // compile: ')'
                var rightParenToken = Eat(")");
            }
            else if (nextToken.Value == ".")
            {
                // compile: (className | varName)
                var nameToken = EatIdentifier();
                var sbClass = SymbolTableManager.Find(nameToken.Value);
                if (sbClass == null)
                {   // handle: class.function(...)
                    sbSubName = nameToken.Value;
                }
                else
                {   // handle: obj.method(...)
                    argadder = 1;
                    sbSubName = sbClass.Type;
                    var sbObj = SymbolTableManager.Find(nameToken.Value);
                    _vmWriter.WritePush(sbObj.KindDisplay, sbObj.Number);
                }

                // compile '.'
                var dotToken = Eat(".");
                sbSubName += dotToken.Value;

                // compile: subroutineName
                var subNameToken = EatIdentifier();
                sbSubName += subNameToken.Value;

                // compile: '('
                var leftParenToken = Eat("(");

                // compile: expression
                args = CompileExpressionList(depth) + argadder;

                // compile: ')'
                var rightParenToken = Eat(")");
            }

            // write call
            _vmWriter.WriteCall(sbSubName, args);
        }

        //
        // expressionList: (expression (',' expression)* )?
        public int CompileExpressionList(int depth)
        {
            int expressionCount = 0;

            // compile: (expression (',' expression)* )?
            if (_tokenizer.CurrentToken.Value != ")")
            {
                CompileExpression(depth + 1);
                expressionCount++;
                while (_tokenizer.CurrentToken.Value == ",")
                {
                    // compile: ','
                    var commaToken = Eat(",");

                    // compile: expression
                    CompileExpression(depth + 1);
                    expressionCount++;
                }
            }

            return expressionCount;
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

            if (result == null)
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
            if (result == null)
            {
                throw new BadSyntaxException();
            }
            return result;
        }

        private Token SoftEatType()
        {
            Token result = null;
            if (_tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.INT
                || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.CHAR
                || _tokenizer.CurrentToken.GetKeywordType() == Types.KeywordType.BOOLEAN)
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
            if (result == null)
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
            if (ops.Contains(_tokenizer.CurrentToken.Value[0]))
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
