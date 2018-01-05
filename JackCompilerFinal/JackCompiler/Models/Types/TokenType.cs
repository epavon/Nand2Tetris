using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    public enum TokenType
    {
        NONE,
        KEYWORD,
        SYMBOL,
        INT_CONST,
        STRING_CONST,
        IDENTIFIER
    }
}
