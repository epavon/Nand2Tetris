using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Helpers
{
    public static class WriterHelper
    {
        public static string GetSpacesDepth(int depth)
        {
            string result = string.Empty;
            for (int i = 0; i < depth * 2; i++)
            {
                result += " ";
            }
            return result;
        }
    }
}
