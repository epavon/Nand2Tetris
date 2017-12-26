using JackCompiler.Writer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JackCompiler.Models.Types;

namespace JackCompiler.Models
{
    public class CompilationUnit
    {
        public string Name { get; set; }
        public List<Token> CompUnits { get; set; }
        public IdentifierType IdentifierType { get; set; }
    }
}
