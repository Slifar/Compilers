using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project
{
    class Token
    {
        public int lineNum;
        public string lexeme;
        public int tokenType;
        public string attribute;
        public string writeToken()
        {
            string toOutput;
            toOutput = lineNum + " " + lexeme + " " + tokenType + " " + attribute;
            return toOutput;
        }
    }
}
