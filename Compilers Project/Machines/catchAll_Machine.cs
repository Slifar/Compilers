using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.Machines
{
    class catchAll_Machine
    {
        public bool Check()
        {
            int state = 0;
            int stateThingFound = 1;
            int EOFState = 2;
            int semicolonState = 3;
            int commaState = 4;
            int colonState = 5;
            int openParensState = 6;
            int closeParensState = 7;
            int doubleDotState = 8;
            
            if (Global_Vars.frontPointer >= Global_Vars.currentLine.Length)
                return false;
            char checking = Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer);

            if (checking == '.')
            {
                if (Global_Vars.frontPointer + 1 < Global_Vars.currentLine.Length)
                {
                    if (Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer + 1) == '.')
                    {
                        state = doubleDotState;
                        Global_Vars.frontPointer++;
                    }
                }
                else state = EOFState;
            }
            else if (checking == ';')
            {
                state = semicolonState;
            }
            else if (checking == ',')
            {
                state = commaState;
            }
            else if (checking == ':')
            {
                state = colonState;
            }
            else if (checking == '(')
            {
                state = openParensState;
            }
            else if (checking == ')')
            {
                state = closeParensState;
            }
            if (state != 0)
            {
                Global_Vars.frontPointer++;
                Global_Vars.backPointer = Global_Vars.frontPointer;
                if (state == semicolonState)
                {
                    Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "semicolon");
                }
                else if (state == EOFState)
                {
                    Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "EOF");
                }
                else if (state == doubleDotState)
                {
                    Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "symbol", "..");
                }
                else
                {
                    Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "symbol", "" + checking);
                }
                return true;
            }
            else return false;
        }
    }
}
