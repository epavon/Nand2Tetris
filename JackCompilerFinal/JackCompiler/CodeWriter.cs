using JackCompilerFinal.Models;
using JackCompilerFinal.Writer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompilerFinal
{
    public class CodeWriter
    {
        private readonly IVmWriter _vmWriter;

        public CodeWriter(IVmWriter vmWriter)
        {
            _vmWriter = vmWriter;
        }

        public void WriteExpr(CompilationUnit compUnit)
        {
            var compUnits = compUnit.CompUnits;
            if (compUnits.Count == 1)
            {
                Token token = compUnits[0];
                if (token.TokenType == TokenType.INT_CONST)
                {
                    _vmWriter.WritePush("constant", Convert.ToInt32(token.Value));
                }
                else if(token.TokenType == TokenType.IDENTIFIER)
                {
                    var sbUnit = SymbolTableManager.Find(token.Value);
                    _vmWriter.WritePush(sbUnit.Kind.ToString(), sbUnit.Number);
                }
            }
        }
    }
}
