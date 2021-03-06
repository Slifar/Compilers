﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Compilers_Project
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.IO.StreamReader program =
                null;
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/
            program = new System.IO.StreamReader(Global_Vars.inputFile);
            File_Reader reservedWordReader = new File_Reader();
            reservedWordReader.Initialize("ReservedWords.txt");
            reservedWordReader.readWords();
            initializeTokens();
            Global_Vars.outputWriter.initialize();
            Global_Vars.outputWriter.initializeTokenOutput(Global_Vars.tokenOutputFile);

            Machines.Addop_Machine addopMachine = new Machines.Addop_Machine();
            Machines.catchAll_Machine catchAllMachine = new Machines.catchAll_Machine();
            Machines.IDRES_Machine IDRESMachine = new Machines.IDRES_Machine();
            Machines.Int_Machine intMachine = new Machines.Int_Machine();
            Machines.Mulop_Machine mulopMachine = new Machines.Mulop_Machine();
            Machines.Real_Machine realMachine = new Machines.Real_Machine();
            Machines.Relop_Machine relopMachine = new Machines.Relop_Machine();
            Machines.Whitespace_Machine whitespaceMachine = new Machines.Whitespace_Machine();
            Machines.longReal_Machine longrealMachine = new Machines.longReal_Machine();

            string line;

            while ((line = program.ReadLine()) != null && !Global_Vars.EOFReached)
            {
                Global_Vars.lines.Add(line);
                Global_Vars.currentLine = line.ToArray();
                Global_Vars.outputWriter.writeLine();
                Boolean charsRemain = true;
                if (line.Length > 72)
                {
                    Global_Vars.outputWriter.writeError(Global_Vars.bufferTooLongError);
                    charsRemain = false;
                    Token token = new Token();
                    token.lineNum = Global_Vars.currentLineNumber;
                    token.lexeme = "";
                    token.tokenType = Global_Vars.lexErrTokenType;
                    token.attribute = Global_Vars.bufferTooLongErrorAttributeNumber;
                    Global_Vars.tokenQueue.Enqueue(token);
                }
                while (charsRemain)
                {
                    Boolean hasPassed = false;
                    hasPassed = whitespaceMachine.check();
                    if (!hasPassed) hasPassed = longrealMachine.Check();
                    if (!hasPassed) hasPassed = realMachine.Check();
                    if (!hasPassed) hasPassed = intMachine.Check();
                    if (!hasPassed) hasPassed = addopMachine.Check();
                    if (!hasPassed) hasPassed = mulopMachine.Check();
                    if (!hasPassed) hasPassed = relopMachine.Check();
                    if (!hasPassed) hasPassed = IDRESMachine.Check();
                    if (!hasPassed) hasPassed = catchAllMachine.Check();
                    if (Global_Vars.backPointer >= Global_Vars.currentLine.Length) charsRemain = false;
                    else if(!hasPassed){
                        Global_Vars.outputWriter.writeError(Global_Vars.unknownSymbolError + "\"" + Global_Vars.currentLine[Global_Vars.backPointer] + "\"");
                        Token token = new Token();
                        token.lineNum = Global_Vars.currentLineNumber;
                        token.lexeme = "" + Global_Vars.currentLine[Global_Vars.backPointer];
                        token.tokenType = Global_Vars.lexErrTokenType;
                        token.attribute = Global_Vars.unrecognizedSymbolAttributeNumber;
                        Global_Vars.tokenQueue.Enqueue(token);
                        Global_Vars.backPointer++;
                        Global_Vars.frontPointer = Global_Vars.backPointer;
                    }
                }
                Global_Vars.currentLineNumber++;
                Global_Vars.backPointer = 0;
                Global_Vars.frontPointer = 0;
            }
            foreach (var thing in Global_Vars.tokenQueue)
            {
                Console.WriteLine(thing.writeToken());
                Global_Vars.outputWriter.writeTokens(thing.writeToken());
            }
            RDParser.Parser parser = new RDParser.Parser();
            parser.parse();
        }

        private static void initializeTokens()
        {
            foreach (var thing in Global_Vars.reservedWords.Keys)
            {
                Word word = Global_Vars.reservedWords[thing];
                Global_Vars.tokenMinter.addToken(
                    word.word,
                    Convert.ToInt32(word.tokenType),
                    word.attribute);
            }
            Global_Vars.tokenMinter.Initialize();
        }
    }
}
