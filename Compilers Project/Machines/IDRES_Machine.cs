using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.Machines
{
    class IDRES_Machine
    {
        

        public bool Check()
        {
            int state = 0;
            int lengthTooLongState = -1;
            int noCharactersState = -2;

            string currentString = "";
            while (true)
            {
                if (Global_Vars.frontPointer >= Global_Vars.currentLine.Length) 
                    break;

                char checking = Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer);
                if (char.IsLetterOrDigit(checking)) //|| checking == '.')
                {
                    currentString += checking;
                    state = 1;
                    Global_Vars.frontPointer++;
                }
                else
                {
                    if (state == 1) state++;
                    break;
                }
            }
            //Global_Vars.backPointer = Global_Vars.frontPointer;
            if (state == 2)
            {
                if (currentString.Length > 10)
                {
                    Global_Vars.outputWriter.writeError(Global_Vars.lengthTooLongError);
                    //Global_Vars.tokenOutput += Global_Vars.currentLineNumber + ", " + currentString + ", 99, 2\n";
                    Token token = new Token();
                    token.lexeme = currentString;
                    token.lineNum = Global_Vars.currentLineNumber;
                    token.tokenType = Global_Vars.lexErrTokenType;
                    token.attribute = Global_Vars.lengthTooLongAttributeNumber;
                    Global_Vars.tokenQueue.Enqueue(token);

                }
                else lookupID(currentString);
                Global_Vars.backPointer = Global_Vars.frontPointer;
                return true;
            }
            else
            {
                Global_Vars.frontPointer = Global_Vars.backPointer;
                return false;
            }
        }

        private void lookupID(string currentString)
        {
            throw new NotImplementedException();
            if (Global_Vars.reservedWords.ContainsKey(currentString))
            {
                Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, currentString);
            }
            else if (Global_Vars.symbolTable.ContainsKey(currentString))
            {
                Global_Vars.tokenMinter.mintNewToken(Global_Vars.currentLineNumber, currentString);
            }
            else
            {
                Word newSymbol = new Word();
                newSymbol.word = currentString;
                newSymbol.tokenType = "" + Global_Vars.idTokenType;
                newSymbol.attribute = "Loc" + Global_Vars.nextSymbolLocation;
                Global_Vars.symbolTable.Add(newSymbol.word, newSymbol);
                Global_Vars.nextSymbolLocation++;
                Global_Vars.tokenMinter.addToken(
                    newSymbol.word, Global_Vars.idTokenType, newSymbol.attribute);
                Global_Vars.tokenMinter.mintNewToken(
                    Global_Vars.currentLineNumber, newSymbol.word);
            }
        }
    }
}
