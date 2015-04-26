using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.RDParser
{
    class greenNode
    {
        public List<blueNode> vars = new List<blueNode>();
        public List<blueNode> parameters = new List<blueNode>();
        public greenNode parent = null;
        public List<greenNode> children = new List<greenNode>();
        public Type type = new Type();
        public string id = "";
    }
}
