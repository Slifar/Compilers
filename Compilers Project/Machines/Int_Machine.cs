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
            while (true)
            {
                if (Global_Vars.frontPointer >= Global_Vars.currentLine.Length)
                    return false;
                char checking = Global_Vars.currentLine.ElementAt(Global_Vars.frontPointer);

            }
        }
    }
}
