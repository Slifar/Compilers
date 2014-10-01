using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.Machines
{
    class Relop_Machine
    {
        public bool Check()
        {
            int state = 0;
            int entryState = 0;
            int greaterThan = 1;
            int lessThan = 2;
            int greaterEquals = 3;
            int lessEquals = 4;
            int assign = 5;
            int equals = 6;
            

            while (true)
            {
                if (Global_Vars.frontPointer >= Global_Vars.currentLine.Length)
                    break;
                char checking = Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer);
                if (state == entryState)
                {
                    #region greaterChecks
                    if (checking == '>')
                    {
                        Global_Vars.frontPointer++;
                        checking = Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer);
                        if (checking == '=')
                        {
                            state = greaterEquals;
                            break;
                        }
                        else
                        {
                            Global_Vars.frontPointer--;
                            state = greaterThan;
                        }

                    }
                    #endregion
                    #region lessChecks
                    else if (checking == '<')
                    {
                        Global_Vars.frontPointer++;
                        checking = Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer);
                        if (checking == '=')
                        {
                            state = lessEquals;
                            break;
                        }
                        else
                        {
                            Global_Vars.frontPointer--;
                            state = lessThan;
                        }
                    }
                    #endregion
                    #region equalsChecks
                    else if (checking == '=')
                    {
                        state = equals;
                        break;
                    }
                    #endregion
                    #region assignChecks
                    else if (checking == ':')
                    {
                        Global_Vars.frontPointer++;
                        checking = Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer);
                        if (checking == '=')
                        {
                            state = assign;
                            break;
                        }
                    }
                    #endregion
                }
                
            }
            if (state == entryState)
            {
                Global_Vars.frontPointer = Global_Vars.backPointer;
                return false;
            }
            else
            {
                if (state == greaterEquals)
                {
                    Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "relop", "greaterEquals");
                    return true;
                }
                else if (state == greaterThan)
                {
                    Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "relop", "greaterThan");
                    return true;
                }
                else if (state == lessThan)
                {
                    Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "relop", "lessThan");
                    return true;
                }
                else if (state == lessEquals)
                {
                    Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "relop", "lessEquals");
                    return true;
                }
                else if (state == assign)
                {
                    Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "relop", "assign");
                    return true;
                }
                else if (state == equals)
                {
                    Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, "relop", "equals");
                    return true;
                }
            }

            Global_Vars.frontPointer = Global_Vars.backPointer;
            return false;
        }
    }
}
