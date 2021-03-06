﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.Machines
{
    class Mulop_Machine
    {
        

        public bool Check()
        {
            int state = 0;
            int entryState = 0;
            int mulState = 1;
            int divState = 2;

            if (Global_Vars.frontPointer >= Global_Vars.currentLine.Length)
                return false;
            char checking = Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer);

            if (checking == '*')
            {
                state = mulState;
                Global_Vars.frontPointer++;
                Global_Vars.backPointer= Global_Vars.frontPointer;
            }
            else if (checking == '/')
            {
                state = mulState;
                Global_Vars.frontPointer++;
                Global_Vars.backPointer = Global_Vars.frontPointer;
            }

            if (state == mulState)
            {
                Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "mulop", "multiply");
                return true;
            }
            else if(state == divState)
            {
                Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "mulop", "divide");
                return true;
            }
            Global_Vars.frontPointer = Global_Vars.backPointer;
            return false;
        }
    }
}
