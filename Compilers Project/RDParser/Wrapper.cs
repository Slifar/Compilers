using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.RDParser
{
    class Wrapper
    {
        public string nonTerminal;
        public string lex;
        public string expected;
        public string productionNum;
        public List<string> follows = new List<string>();
        public int lexeme;
        public int line;
        public Type type = new Type();
        public List<Wrapper> children = new List<Wrapper>();
    }
}
