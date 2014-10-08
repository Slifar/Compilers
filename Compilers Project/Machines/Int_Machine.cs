using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.Machines
{
    class Int_Machine
    {
        public bool Check()
        {
            int state = 0;
            int entryState = 0;
            int haveNumberState = 1;

            bool firstDigitZero = false;

            string currentString = "";
            while (true)
            {
                if (Global_Vars.frontPointer >= Global_Vars.currentLine.Length)
                    break;
                char checking = Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer);

                #region entryState
                if (state == entryState)
                {
                    if (char.IsDigit(checking))
                    {
                        state = haveNumberState;
                        currentString += checking;
                        Global_Vars.frontPointer++;
                        if (checking == '0')
                        {
                            firstDigitZero = true;
                        }
                    }
                    else break;
                }
                #endregion
                else if (state == haveNumberState)
                {
                    if (char.IsDigit(checking))
                    {
                        currentString += checking;
                        Global_Vars.frontPointer++;
                    }
                    else break;
                }
                else break;
            }
            if (state != entryState)
            {
                Global_Vars.backPointer = Global_Vars.frontPointer;
                if (currentString.Length > Global_Vars.Max_Int_Length)
                {

                }
                return true;
            }
            else
            {
                Global_Vars.frontPointer = Global_Vars.backPointer;
                return false;
            }
        }
    }
}
