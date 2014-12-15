using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project
{
    class File_Writer
    {
        public void initialize()
        {
            throw new NotImplementedException();
        }
        public void writeError(string toWrite)
        {
            string finalOutput = "Line " + Global_Vars.currentLineNumber + ": " + toWrite;
            throw new NotImplementedException();
        }
    }
}
