﻿using JackCompiler.Models;
using JackCompiler.Writer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
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
                var token = compUnits[0];
                if (token.TokenType == TokenType.INT_COSNT)
                {
                    var sbUnit = SymbolTableManager.Find(token.Value);
                    _vmWriter.WritePush(sbUnit.Kind.ToString(), sbUnit.Number);
                }
                else if(token.TokenType == TokenType.IDENTIFIER)
                {

                }
            }
        }
    }
}