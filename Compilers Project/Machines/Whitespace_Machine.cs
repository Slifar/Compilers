using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.Machines
{
    class Whitespace_Machine
    {
        public bool check()
        {
            int state = 0;
            while (true)
            {
                if (Global_Vars.frontPointer >= Global_Vars.currentLine.Length)
                    break;
                char checking = Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer);
                if (char.IsWhiteSpace(checking))
                {
                    state = 1;
                    Global_Vars.frontPointer++;
                }
                else break;
            }
            if (state == 1)
            {
                Global_Vars.backPointer = Global_Vars.frontPointer;
                return true;
            }
            else return false;
        }
    }
}
