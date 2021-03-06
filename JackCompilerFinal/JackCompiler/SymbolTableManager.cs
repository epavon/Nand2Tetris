﻿using JackCompilerFinal.Models;
using JackCompilerFinal.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompilerFinal
{
    public static class SymbolTableManager
    {
        private static Dictionary<VarScopeType, List<SymbolTableItem>> SymbolTable;

        static SymbolTableManager()
        {
            SymbolTable = new Dictionary<VarScopeType, List<SymbolTableItem>>() { { VarScopeType.CLASS_LEVEL, new List<SymbolTableItem>() }, { VarScopeType.SUBROUTINE_LEVEL, new List<SymbolTableItem>() } };
        }

        public static void AddToClassSymbolTable(SymbolTableItem symbolTableItem)
        {
            var symbolTableItemNum = SymbolTable[VarScopeType.CLASS_LEVEL].Count(sti => sti.Kind == symbolTableItem.Kind);
            symbolTableItem.Number = symbolTableItemNum;
            SymbolTable[VarScopeType.CLASS_LEVEL].Add(symbolTableItem);
        }

        public static void ResetClassSymbolTable()
        {
            SymbolTable[VarScopeType.CLASS_LEVEL] = new List<SymbolTableItem>();
        }

        public static void AddToSubroutineSymbolTable(SymbolTableItem symbolTableItem)
        {
            var symbolTableItemNum = SymbolTable[VarScopeType.SUBROUTINE_LEVEL].Count(sti => sti.Kind == symbolTableItem.Kind);
            symbolTableItem.Number = symbolTableItemNum;
            SymbolTable[VarScopeType.SUBROUTINE_LEVEL].Add(symbolTableItem);
        }

        public static void ResetSubroutineSymbolTable()
        {
            SymbolTable[VarScopeType.SUBROUTINE_LEVEL] = new List<SymbolTableItem>();
        }

        public static SymbolTableItem Find(string varName)
        {
            SymbolTableItem result = null;

            result = SymbolTable[VarScopeType.SUBROUTINE_LEVEL].FirstOrDefault(si => si.Name == varName) ??
                        SymbolTable[VarScopeType.CLASS_LEVEL].FirstOrDefault(si => si.Name == varName);

            return result;
        }

        public static int GetLocalsNum()
        {
            int result = 0;

            result = SymbolTable[VarScopeType.SUBROUTINE_LEVEL].Count(si => si.KindDisplay == "local");

            return result;
        }

        public static int GetFieldsNum()
        {
            int result = SymbolTable[VarScopeType.CLASS_LEVEL].Count(si => si.KindDisplay == "this");
            return result;
        }
    }
}
