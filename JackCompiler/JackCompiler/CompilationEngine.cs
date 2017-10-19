using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    public class CompilationEngine
    {
        StreamWriter _streamWriter;


        //
        // Ctors / Dtors
        //
        public CompilationEngine(string fileName)
        {
            _streamWriter = new StreamWriter(fileName);
        }

        ~CompilationEngine()
        {
            if(_streamWriter != null)
            {
                _streamWriter.Dispose();
                _streamWriter = null;
            }
        }

        //
        // Methods
        //

        public void CompileClass()
        {

        }

        public void CompileStatements()
        {

        }

        public void CompileIfStatement()
        {

        }

        public void CompileWhileStatement()
        {

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
