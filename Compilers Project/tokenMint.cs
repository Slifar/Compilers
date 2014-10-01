using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project
{
    class tokenMint
    {
        public static Dictionary<string, Token> masterTokens = new Dictionary<string, Token>();
        public void addToken(string lexeme, int tokenType, string defaultAttribute)
        {
            Token temp = new Token();
            temp.attribute = defaultAttribute;
            temp.lexeme = lexeme;
            temp.lineNum = -1;
            temp.tokenType = tokenType;
        }
        public void mintNewToken(int lineNumber, string tokenName)
        {
            if (masterTokens.ContainsKey(tokenName))
            {
                Token temp = new Token();
                Token master = masterTokens[tokenName];
                temp.lexeme = master.lexeme;
                temp.lineNum = lineNumber;
                temp.tokenType = master.tokenType;
                temp.attribute = master.attribute;
                Global_Vars.tokenQueue.Enqueue(temp);

            }
            else
            {
                Console.WriteLine("Error: No token with the name :" + tokenName);
            }
        }
        public void mintNewToken(int lineNumber, string tokenName, string attribute)
        {
            if (masterTokens.ContainsKey(tokenName))
            {
                Token temp = new Token();
                Token master = masterTokens[tokenName];
                temp.lexeme = master.lexeme;
                temp.lineNum = lineNumber;
                temp.tokenType = master.tokenType;
                temp.attribute = attribute;
                Global_Vars.tokenQueue.Enqueue(temp);

            }
            else
            {
                Console.WriteLine("Error: No token with the name :" + tokenName);
            }
        }
        #region Token initialization
        public void Initialize()
        {
            this.addToken("relop", Global_Vars.relopTokenType, "null");
            this.addToken("addop", Global_Vars.addopTokenType, "null");
            this.addToken("mulop", Global_Vars.mulopTokenType, "null");
        }
        #endregion
    }
}
