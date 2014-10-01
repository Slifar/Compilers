using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project
{
    class Word
    {
        public string word;
        public string tokenType;
        public string attribute;
        public string output()
        {
            return this.word + " " + this.tokenType + " " + this.attribute;
        }
    }
}
