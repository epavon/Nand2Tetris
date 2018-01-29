using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JackCompilerFinal.Types
{
    public static class IdentifierHelper
    {
        public static Regex IdentifierRegex { get; private set; }

        static IdentifierHelper()
        {
            IdentifierRegex = new Regex(@"^([a-zA-Z_]+[A-Za-z0-9_]*)$");
        }
    }
}
