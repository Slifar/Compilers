using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project
{
    class File_Writer
    {
        System.IO.StreamWriter file = null;
        public void initialize()
        {
            file = new System.IO.StreamWriter(Global_Vars.OutputFile);
        }
        public void writeError(string toWrite)
        {
            string finalOutput = toWrite;
            file.WriteLine(finalOutput);
            file.Flush();
            //throw new NotImplementedException();
        }
        public void writeLine()
        {
            file.WriteLine();
            string outputLine = new String(Global_Vars.currentLine);
            string finalOutput = "Line " + Global_Vars.currentLineNumber + ": " + outputLine;
            file.WriteLine(finalOutput);
            file.Flush();
        }
    }
}
