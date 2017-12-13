using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    public static class SettingsReader
    {
        public static readonly string _appSettingsPath;

        private static List<string> keywords = new List<string> { "class", "constructor", "function", "method", "field", "static", "var", "int", "char", "boolean", "void", "true", "false", "null", "this", "let", "do", "if", "else", "while", "return" };
        private static List<char> symbols = new List<char> { '{', '}', '(', ')', '[', ']', '.', ',', ';', '+', '-', '*', '/', '&', '|', '<', '>', '=', '~' };


        public static List<string> Keywords
        {
            get
            {
                return keywords;
            }
        }

        public static List<char> Symbols
        {
            get
            {
                return symbols;
            }
        }

    }
}
