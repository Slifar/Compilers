using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project
{
    class Global_Vars
    {
        public static int Max_buffer = 72;
        public static int frontPointer = 0;
        public static int backPointer = 0;
        public static int Max_Ident_Length = 10;
        public static int Max_Int_Length = 10;
        public static int Max_Real_Front = 5;
        public static int Max_Real_Back = 5;
        public static int Max_Real_Power = 2;

        public static int nextSymbolLocation = 0;

        public static int currentLineNumber = 1;
        public static string ReservedWordFile;
        public static string OutputFile;
        public static Dictionary<string, Word> reservedWords = new Dictionary<string,Word>();
        public static Dictionary<string, Word> symbolTable = new Dictionary<string, Word>();
        public static File_Writer outputWriter = new File_Writer();
        public static char[] currentLine;
        public static tokenMint tokenMinter = new tokenMint();

        public const string lengthTooLongError = "Lexical Error 1: identifier length too long.";
        public const string unknownSymbolError = "Lexical Error 2: Unknown Symbol: ";
        public const string intTooLongError = "Lexical Error 3: Integer has too many digits.";
        public const string preDecimalRealTooLongError = "Lexical Error 4: Too many digits before the decimal.";
        public const string postDecimalRealTooLongError = "Lexical Error 5: Too many digits after the decimal.";
        public const string intLeadingZeroesError = "Lexical Error 6: Number has leading zeroes.";
        public const string realTrailingZeroesError = "Lexical Error 7: Number has trailing zeroes.";

        public static string tokenOutput = "Line No, Lexeme, Token-Type, Attribute\n";
        public static Queue<Token> tokenQueue = new Queue<Token>();


        #region Token Type Ints
        public const int relopTokenType = 1;
        public const int mulopTokenType = 2;
        public const int addopTokenType = 3;
        public const int intTokenType = 4;
        public const int realTokenType = 5;
        public const int longrealTokenType = 6;
        public const int idTokenType = 7;
        
        public static int lexErrTokenType = 99;
        public static string lengthTooLongAttributeNumber = "2";
        public static string unrecognizedSymbolAttributeNumber = "1";
        public static string intTooLongAttributeNumber = "3";
        public static string preDecimalRealTooLongAttributeNumber = "4";
        public static string postDecimalRealTooLongAttributeNumber = "5";

        #endregion

    }
}
