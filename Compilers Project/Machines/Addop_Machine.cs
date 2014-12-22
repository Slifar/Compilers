using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.Machines
{
    class Addop_Machine
    {
        

        public bool Check()
        {
            int state = 0;
            int entryState = 0;
            int addState = 1;
            int subState = 2;

            if (Global_Vars.frontPointer >= Global_Vars.currentLine.Length)
                return false;
            char checking = Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer);

            if (checking == '+')
            {
                state = addState;
                Global_Vars.frontPointer++;
                Global_Vars.backPointer = Global_Vars.frontPointer;
            }
            else if (checking == '-')
            {
                state = subState;
                Global_Vars.frontPointer++;
                Global_Vars.backPointer = Global_Vars.frontPointer;
            }

            if (state == addState)
            {
                Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "addop", "add");
                return true;
            }
            else if (state == subState)
            {
                Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "addop", "subtract");
                return true;
            }
            Global_Vars.frontPointer = Global_Vars.backPointer;
            return false;
        }
    }
}
