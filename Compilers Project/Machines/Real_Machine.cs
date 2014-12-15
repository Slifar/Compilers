using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.Machines
{
    class Real_Machine
    {
        public bool Check()
        {
            int state = 0;
            int entryState = 0;
            int preDecimalState = 1;
            int postDecimalState = 2;

            bool firstDigitZero = false;
            bool lastDigitZero = false;

            string currentString = "";
            int preDecimalLength = 0;
            int postDecimalLength = 0;

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
                        state = preDecimalState;
                        currentString += checking;
                        Global_Vars.frontPointer++;
                        if (checking == '0')
                        {
                            firstDigitZero = true;
                        }
                        preDecimalLength++;
                    }
                    else break;
                }
                #endregion
                else if (state == preDecimalState)
                {
                    if (char.IsDigit(checking))
                    {
                        currentString += checking;
                        preDecimalLength++;
                        Global_Vars.frontPointer++;
                    }
                    else if (checking == '.')
                    {
                        currentString += checking;
                        Global_Vars.frontPointer++;
                        state = postDecimalState;
                    }
                    else break;
                }
                else if (state == postDecimalState)
                {
                    if (char.IsDigit(checking))
                    {
                        currentString += checking;
                        postDecimalLength++;
                        Global_Vars.frontPointer++;
                    }
                }
                else break;
            }
            if (state == postDecimalState)
            {
                Global_Vars.backPointer = Global_Vars.frontPointer;
                if (currentString.Length > Global_Vars.Max_Int_Length)
                {
                    Global_Vars.outputWriter.writeError(Global_Vars.intTooLongError);
                }
                else if (currentString.Length > 1 && firstDigitZero)
                {
                    Global_Vars.outputWriter.writeError(Global_Vars.intLeadingZeroesError);
                }
                else
                {
                    Token token = new Token();
                    token.lineNum = Global_Vars.currentLineNumber;
                    token.lexeme = currentString;
                    token.tokenType = Global_Vars.realTokenType;
                    token.attribute = "null";
                    Global_Vars.tokenQueue.Enqueue(token);
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
