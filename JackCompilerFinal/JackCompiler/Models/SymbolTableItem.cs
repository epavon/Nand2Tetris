using JackCompilerFinal.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompilerFinal.Models
{
    public class SymbolTableItem
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public VarKindType Kind { get; set; }
        public VarScopeType Scope { get; set; }

        public string KindDisplay
        {
            get
            {
                switch(Kind)
                {
                    case VarKindType.FIELD:
                        return "this";
                    case VarKindType.LOCAL:
                        return "local";
                    case VarKindType.ARGUMENT:
                        return "argument";
                    case VarKindType.STATIC:
                        return "static";
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
