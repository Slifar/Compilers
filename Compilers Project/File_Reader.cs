using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project
{
    class File_Reader
    {
        string toRead = null;
        System.IO.StreamReader reader =
            null;
        public void Initialize(string filename)
        {
            toRead = filename;
            reader = new System.IO.StreamReader(toRead);
        }
        public void readWords()
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Word word = new Word();
                string[] split = line.Split(null);
                word.word = split[0];
                word.tokenType = split[1];
                word.attribute = split[2];
                Global_Vars.reservedWords.Add(word.word, word);
            }
            #region adding div, or, mod, and
            Word divWord = new Word();
            divWord.word = "div";
            divWord.tokenType = "" + Global_Vars.mulopTokenType;
            divWord.attribute = "null";
            Global_Vars.reservedWords.Add(divWord.word, divWord);
            divWord = new Word();
            divWord.word = "or";
            divWord.tokenType = "" + Global_Vars.addopTokenType;
            divWord.attribute = "null";
            Global_Vars.reservedWords.Add(divWord.word, divWord);
            divWord = new Word();
            divWord.word = "mod";
            divWord.tokenType = "" + Global_Vars.mulopTokenType;
            divWord.attribute = "null";
            Global_Vars.reservedWords.Add(divWord.word, divWord);
            divWord = new Word();
            divWord.word = "and";
            divWord.tokenType = "" + Global_Vars.mulopTokenType;
            divWord.attribute = "null";
            Global_Vars.reservedWords.Add(divWord.word, divWord);
            #endregion
        }
    }
}
