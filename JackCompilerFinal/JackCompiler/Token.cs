using JackCompilerFinal.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompilerFinal
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
                    return "&lt;";
                if (Value == ">")
                    return "&gt;";
                if (Value == "&")
                    return "&amp;";
                return Value;
            } 
        }

        public KeywordType GetKeywordType()
        {
            KeywordType thisKeywordType = KeywordType.NONE;

            if(TokenType == JackCompilerFinal.TokenType.KEYWORD)
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
                case JackCompilerFinal.TokenType.IDENTIFIER:
                    return "identifier";
                case JackCompilerFinal.TokenType.INT_CONST:
                    return "integerConstant";
                case JackCompilerFinal.TokenType.KEYWORD:
                    return "keyword";
                case JackCompilerFinal.TokenType.STRING_CONST:
                    return "stringConstant";
                case JackCompilerFinal.TokenType.SYMBOL:
                    return "symbol";
            }
            return string.Empty;
        }
    }
}
