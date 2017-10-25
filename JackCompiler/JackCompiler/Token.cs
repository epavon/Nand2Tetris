using JackCompiler.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    public class Token
    {
        public string Value { get; set; }
        
        public TokenType TokenType { get; set; }

        public string OutputValue 
        { 
            get 
            {
                if (Value == "<")
                    return "&lt";
                if (Value == ">")
                    return "&gt";
                return Value;
            } 
        }

        public KeywordType GetKeywordType()
        {
            KeywordType thisKeywordType = KeywordType.NONE;

            if(TokenType == JackCompiler.TokenType.KEYWORD)
            {
                var keywordTypeValues = Enum.GetValues(typeof(KeywordType));
                foreach (var kwType in keywordTypeValues)
                {
                    if (kwType.ToString() == Value.ToUpper())
                    {
                        thisKeywordType = (KeywordType)kwType;
                    }
                }
            }

            return thisKeywordType;
        }

        public string GetTokenTypeName()
        {
            switch(TokenType)
            {
                case JackCompiler.TokenType.IDENTIFIER:
                    return "identifier";
                case JackCompiler.TokenType.INT_COSNT:
                    return "integerConstant";
                case JackCompiler.TokenType.KEYWORD:
                    return "keyword";
                case JackCompiler.TokenType.STRING_CONST:
                    return "stringConstant";
                case JackCompiler.TokenType.SYMBOL:
                    return "symbol";
            }
            return string.Empty;
        }
    }
}
