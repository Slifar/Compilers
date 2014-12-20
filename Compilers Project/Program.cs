using System;
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            File_Reader reservedWordReader = new File_Reader();
            reservedWordReader.Initialize("ReservedWords.txt");
            reservedWordReader.readWords();
            initializeTokens();
        }

        private static void initializeTokens()
        {
            throw new NotImplementedException();
        }
    }
}
