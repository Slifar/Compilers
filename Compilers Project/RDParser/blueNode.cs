﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.RDParser
{
    class blueNode
    {
        public Type type = new Type();
        public string id = "";
        public int leftIndex = 0;
        public int rightIndex = 0;
        public int memLoc;

        public string makeString()
        {
            string toReturn = "var id: " + this.id + "; type: " + this.type.type + "; location: " + this.memLoc + ".";
            return toReturn;
        }
    }
}
