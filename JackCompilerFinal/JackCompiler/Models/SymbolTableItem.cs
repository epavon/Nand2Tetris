﻿using JackCompiler.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Models
{
    public class SymbolTableItem
    {
        public string Name { get; set; }
        public VarTypeType Type { get; set; }
        public VarKindType Kind { get; set; }
        public VarScopeType Scope { get; set; }
    }
}
