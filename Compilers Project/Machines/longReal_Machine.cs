using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.Machines
{
    class longReal_Machine
    {
        public bool Check()
        {
            int state = 0;
            int entryState = 0;
            int preDecimalState = 1;
            int postDecimalState = 2;
            int postEState = 3;
            int postEDigitState = 4;

            bool firstDigitZero = false;
            bool lastDigitZero = false;

            string currentString = "";
            int preDecimalLength = 0;
            int postDecimalLength = 0;
            int postELength = 0;

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
                        if (checking == '0')
                        {
                            lastDigitZero = true;
                        }
                        else lastDigitZero = false;
                        currentString += checking;
                        postDecimalLength++;
                        Global_Vars.frontPointer++;
                    }
                    else if (checking == 'E')
                    {
                        state = postEState;
                        currentString += checking;
                        Global_Vars.frontPointer++;
                    }
                    else break;
                }
                else if (state == postEState)
                {
                    if (char.IsDigit(checking))
                    {
                        postELength++;
                        currentString += checking;
                        Global_Vars.frontPointer++;
                        state = postEDigitState;
                    }
                    else break;
                }
                else if (state == postEDigitState)
                {
                    if (char.IsDigit(checking))
                    {
                        postELength++;
                        currentString += checking;
                        Global_Vars.frontPointer++;
                        state = postEDigitState;
                    }
                    else break;
                }
                else break;
            }
            if (state == postEDigitState)
            {
                Global_Vars.backPointer = Global_Vars.frontPointer;
                if (preDecimalLength > Global_Vars.Max_Real_Front)
                {
                    Global_Vars.outputWriter.writeError(Global_Vars.preDecimalRealTooLongError);
                    Token token = new Token();
                    token.lineNum = Global_Vars.currentLineNumber;
                    token.lexeme = currentString;
                    token.tokenType = Global_Vars.lexErrTokenType;
                    token.attribute = Global_Vars.preDecimalRealTooLongAttributeNumber;
                    Global_Vars.tokenQueue.Enqueue(token);
                }
                else if (postELength > Global_Vars.Max_Real_Power)
                {
                    Global_Vars.outputWriter.writeError(Global_Vars.realPowerTooLongError);
                    Token token = new Token();
                    token.lineNum = Global_Vars.currentLineNumber;
                    token.lexeme = currentString;
                    token.tokenType = Global_Vars.lexErrTokenType;
                    token.attribute = Global_Vars.realPowerTooLongErrorAttributeNumber;
                    Global_Vars.tokenQueue.Enqueue(token);
                }
                else if (postDecimalLength > Global_Vars.Max_Real_Back)
                {
                    Global_Vars.outputWriter.writeError(Global_Vars.postDecimalRealTooLongError);
                    Token token = new Token();
                    token.lineNum = Global_Vars.currentLineNumber;
                    token.lexeme = currentString;
                    token.tokenType = Global_Vars.lexErrTokenType;
                    token.attribute = Global_Vars.postDecimalRealTooLongAttributeNumber;
                    Global_Vars.tokenQueue.Enqueue(token);
                }
                else if (preDecimalLength > 1 && firstDigitZero)
                {
                    Global_Vars.outputWriter.writeError(Global_Vars.intLeadingZeroesError);
                    Token token = new Token();
                    token.lineNum = Global_Vars.currentLineNumber;
                    token.lexeme = currentString;
                    token.tokenType = Global_Vars.lexErrTokenType;
                    token.attribute = Global_Vars.leadingZeroesErrorAttributeNumber;
                    Global_Vars.tokenQueue.Enqueue(token);
                }
                else if (currentString.Length > 1 && firstDigitZero)
                {
                    Global_Vars.outputWriter.writeError(Global_Vars.realTrailingZeroesError);
                    Token token = new Token();
                    token.lineNum = Global_Vars.currentLineNumber;
                    token.lexeme = currentString;
                    token.tokenType = Global_Vars.lexErrTokenType;
                    token.attribute = Global_Vars.trailingZeroesErrorAttributeNumber;
                    Global_Vars.tokenQueue.Enqueue(token);
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
