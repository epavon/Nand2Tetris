using JackCompilerFinal.Writer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompilerFinal.Models
{
    public class CompilationUnit
    {
        public string Name { get; set; }
        public List<Token> CompUnits { get; set; }
        
    }
}
