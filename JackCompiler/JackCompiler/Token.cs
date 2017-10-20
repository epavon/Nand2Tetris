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


    }
}
