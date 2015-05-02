using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.RDParser
{
    class Parser
    {
        Queue<Token> tokenQueue = Global_Vars.tokenQueue;
        int offset = 0;
        int currLine = 1;
        string currentId = "";
        int currentParam = 0;
        int firstArrayIndex = 0;
        int secondArrayIndex = 0;
        Stack<greenNode> greenNodeStack = new Stack<greenNode>();
        blueNode currentBlueNode = null;
        greenNode currentGreenNode = null;
        Type leftType = null;
        bool doubleCheck = false;
        System.IO.StreamWriter output = new System.IO.StreamWriter("ErrorOutputs.txt");

        public void parse()
        {
           // Queue<Token> tokenQueue = Global_Vars.tokenQueue;

            parseProgram();
            
        }

        public void reportParserError(string expected, Token next)
        {
            string output = "";
            if (next.tokenType == 99)
            {
                if (next.attribute == "1")
                {
                    output = "Lexical error on line " + next.lineNum + ": Length too long.";
                }
                else if (next.attribute == "2")
                {
                    output = "Lexical error on line " + next.lineNum + ": Unrecognize symbol "+ next.lexeme + ".";
                }
                else if (next.attribute == "3")
                {
                    output = "Lexical error on line " + next.lineNum + ": integer too long.";
                }
                else if (next.attribute == "4")
                {
                    output = "Lexical error on line " + next.lineNum + ": too many digits before the decimal";
                }
                else if (next.attribute == "5")
                {
                    output = "Lexical error on line " + next.lineNum + ": too many digits after the decimal";
                }
                else if (next.attribute == "6")
                {
                    output = "Lexical error on line " + next.lineNum + ": leaing zeroes error";
                }
                else if (next.attribute == "7")
                {
                    output = "Lexical error on line " + next.lineNum + ": trailing zeroes error";
                }
                else if (next.attribute == "8")
                {
                    output = "Lexical error on line " + next.lineNum + ": line too long error";
                }
                else if (next.attribute == "9")
                {
                    output = "Lexical error on line " + next.lineNum + ": real power too long error";
                }
            }
            else
            {
                output = "Syntax Error on line " + next.lineNum + ": Expected " + expected + ", got: " + next.lexeme + ".";
                
            }
            if (next.lineNum != currLine)
            {
                currLine = next.lineNum;
                //fileWrite(Global_Vars.lines.ElementAt(currLine - 1));
            }
            Console.WriteLine(output);
            fileWrite(output);
        }

        public void fileWrite(String toWrite)
        {
            //output.WriteLine(toWrite);
            //output.Flush();
            Global_Vars.lines[currLine-1] += System.Environment.NewLine + toWrite;
        }

        public void finalWrite()
        {
            foreach (string str in Global_Vars.lines)
            {
               output.WriteLine(str);
               output.Flush();
            }
        }
        public Wrapper parseProgram()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "program";
            toReturn.expected = "program";
            toReturn.follows.Add("$");
            Token next = tokenQueue.Dequeue();
            if (next.tokenType == 10)
            {
                next = tokenQueue.Dequeue();
                if (next.tokenType == 7)
                {
                    addGreenNode(next.lexeme, "PROGNAME");
                    offset = 0;
                    next = tokenQueue.Dequeue();
                    if (next.tokenType == 9 && next.attribute.ToLower().Equals("("))
                    {
                        Wrapper idList = parseIdentifierList();
                        toReturn.children.Add(idList);
                        next = tokenQueue.Dequeue();
                        if (idList.type.type == "ERR" || idList.type.type == "ERR*")
                        {
                            toReturn.type.type = "ERR";
                        }
                        else if (next.tokenType == 9 && next.attribute.ToLower().Equals(")"))
                        {
                            next = tokenQueue.Dequeue();
                            if (next.tokenType == 8)
                            {
                                Wrapper program2 = parseProgram2();
                                toReturn.children.Add(program2);
                                if (program2.type.type == "ERR" || idList.type.type == "ERR*")
                                {
                                    toReturn.type.type = "ERR";
                                }
                                outputMemories();
                            }
                            else
                            {
                                reportParserError(";", next);
                                toReturn.type.type = "ERR*";
                            }
                        }
                        else
                        {
                            reportParserError(")", next);
                            toReturn.type.type = "ERR*";
                        }
                    }
                    else
                    {
                        reportParserError("(", next);
                        toReturn.type.type = "ERR*";
                    }
                }
                else
                {
                    reportParserError("an id", next);
                    toReturn.type.type = "ERR*";
                }
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
            }
            finalWrite();
            return toReturn;
        }

        private Wrapper parseProgram2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "program'";
            toReturn.expected = "procedure, begin, or var";
            toReturn.follows.Add("$");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 16 || next.tokenType == 17) // Production 1.3
            {
                toReturn.productionNum = "1.3";
                Wrapper subprogramDeclarations = parseSubprogramDecarations();
                toReturn.children.Add(subprogramDeclarations);
                if (subprogramDeclarations.type.type == "ERR" || subprogramDeclarations.type.type == "ERR*")
                {
                    toReturn.type.type = "ERR";
                }
                Wrapper compoundStatement = parseCompoundStatement();
                toReturn.children.Add(compoundStatement);
                if (compoundStatement.type.type == "ERR" || compoundStatement.type.type == "ERR*")
                {
                    toReturn.type.type = "ERR";
                }
                next = tokenQueue.Dequeue();
                if (next.tokenType != 98)
                {
                    reportParserError(".", next);
                }
            }
            else if (next.tokenType == 11)// Production 1.2
            {
                toReturn.productionNum = "1.2";
                Wrapper declarations = parseDeclarations();
                toReturn.children.Add(declarations);
                if (declarations.type.type == "ERR" || declarations.type.type == "ERR*")
                {
                    toReturn.type.type = "ERR";
                }
                Wrapper subprogramDeclarations = parseSubprogramDecarations();
                toReturn.children.Add(subprogramDeclarations);
                if (subprogramDeclarations.type.type == "ERR" || subprogramDeclarations.type.type == "ERR*")
                {
                    toReturn.type.type = "ERR";
                }
                Wrapper compoundStatement = parseCompoundStatement();
                toReturn.children.Add(compoundStatement);
                if (compoundStatement.type.type == "ERR" || compoundStatement.type.type == "ERR*")
                {
                    toReturn.type.type = "ERR";
                }
                next = tokenQueue.Dequeue();
                if (next.tokenType != 98)
                {
                    reportParserError(".", next);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
            }
            return toReturn;
        }

        private Wrapper parseIdentifierList()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "identifier_list";
            toReturn.expected = "an id";
            toReturn.follows.Add(")");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 7)
            {
                tokenQueue.Dequeue();
                if (currLine != next.lineNum)
                {
                    currLine = next.lineNum;
                }
                if (!addBlueNode(next.lexeme, "PGPARAM"))
                {
                    string toWrite = "\t" + next.lineNum.ToString() + ". Errored lexeme: " + next.lexeme + ".";
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                    toReturn.type.type = "ERR*";
                }
                Wrapper identifierList2 = parseIdentifierList2();
                errorCheck(toReturn, identifierList2);
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseIdentifierList2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "identifier_list'";
            toReturn.expected = ", or )";
            toReturn.follows.Add(")");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 9)
            {
                if (next.attribute.Equals(","))//2.2
                {
                    toReturn.productionNum = "2.2";
                    tokenQueue.Dequeue();
                    if (tokenQueue.Peek().tokenType == 7)
                    {
                        next = tokenQueue.Dequeue();
                        if (currLine != next.lineNum)
                        {
                            currLine = next.lineNum;
                        }
                        if (!addBlueNode(next.lexeme, "PGPARAM"))
                        {
                            string toWrite = "\t" + next.lineNum.ToString() + ". Errored lexeme: " + next.lexeme + ".";
                            fileWrite(toWrite);
                            Console.WriteLine(toWrite);
                            toReturn.type.type = "ERR*";
                        }
                        Wrapper identifierList2 = parseIdentifierList2();
                        errorCheck(toReturn, identifierList2);
                    }
                    else
                    {
                        reportParserError("an id", next);
                        toReturn.type.type = "ERR*";
                    }
                }
                else if (next.attribute.Equals(")"))//2.3. We get the follows because we are producing epsilon
                {
                    toReturn.productionNum = "2.3";
                    //Epsilon prouction. do nothing!
                }
                else
                {
                    reportParserError(toReturn.expected, next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }
        private Wrapper parseDeclarations()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "declarations";
            toReturn.expected = "var, procedure, or begin";
            toReturn.follows.Add("procedure");
            toReturn.follows.Add("begin");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 11)
            {
                tokenQueue.Dequeue();
                next = tokenQueue.Peek();
                if (next.tokenType == 7)
                {
                    string id = next.lexeme;
                    tokenQueue.Dequeue();
                    next = tokenQueue.Peek();
                    if (next.tokenType == 9 && next.attribute == ":")
                    {
                        tokenQueue.Dequeue();
                        Wrapper type = parseType();
                        errorCheck(toReturn, type);
                        if (currLine != next.lineNum)
                        {
                            currLine = next.lineNum;
                        }
                        if (!addBlueNode(id, type.type.type))
                        {
                            string toWrite = "\t" + next.lineNum.ToString() + ". Errored lexeme: " + id + ".";
                            fileWrite(toWrite);
                            Console.WriteLine(toWrite);
                            toReturn.type.type = "ERR*";
                        }
                        next = tokenQueue.Peek();
                        if (next.tokenType == 8)
                        {
                            tokenQueue.Dequeue();
                            Wrapper declarations = parseDeclarations();
                            errorCheck(toReturn, declarations);
                        }
                        else
                        {
                            reportParserError(";", next);
                            toReturn.type.type = "ERR*";
                            errorRecov(toReturn);
                        }
                    }
                    else
                    {
                        reportParserError(":", next);
                        toReturn.type.type = "ERR*";
                        errorRecov(toReturn);
                    }
                }
                else
                {
                    reportParserError("an id", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else if (toReturn.follows.Contains(next.lexeme))
            {
                //Epsilon production, do nothing. 
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseType()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "type";
            toReturn.expected = "array, integer, or real";
            toReturn.follows.Add(";");
            toReturn.follows.Add(")");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 14 || next.tokenType == 15)
            {
                toReturn.productionNum = "4.1";
                Wrapper standardType = parseStandardType();
                errorCheck(toReturn, standardType);
                if (toReturn.type.type != "ERR")
                {
                    toReturn.type.type = standardType.type.type;
                    toReturn.type.size = standardType.type.size;
                }
            }
            else if (next.tokenType == 12)
            {
                toReturn.productionNum = "4.2";
                tokenQueue.Dequeue();
                next = tokenQueue.Peek();
                if (next.tokenType == 9 && next.attribute == "[")
                {
                    tokenQueue.Dequeue();
                    next = tokenQueue.Peek();
                    if (next.tokenType == Global_Vars.intTokenType)
                    {
                        tokenQueue.Dequeue();
                        string temp = next.lexeme;
                        int firstNum = Convert.ToInt32(temp);
                        next = tokenQueue.Peek();
                        if (next.tokenType == 9 && next.attribute == "..")
                        {
                            tokenQueue.Dequeue();
                            next = tokenQueue.Peek();
                            if (next.tokenType == Global_Vars.intTokenType)
                            {
                                tokenQueue.Dequeue();
                                int secondNum = Convert.ToInt32(next.lexeme);
                                next = tokenQueue.Peek();
                                if (next.tokenType == 9 && next.attribute == "]")
                                {
                                    tokenQueue.Dequeue();
                                    next = tokenQueue.Peek();
                                    if (next.tokenType == 13)
                                    {
                                        tokenQueue.Dequeue();
                                        Wrapper standardType = parseStandardType();
                                        if (firstNum >= secondNum)
                                        {
                                            toReturn.type.type = "ERR*";
                                            String toWrite = "Semantic error on line: " + next.lineNum + ", improper array indicies";
                                            Console.WriteLine(toWrite);
                                            if (next.lineNum != currLine)
                                            {
                                                currLine = next.lineNum;
                                            }
                                            fileWrite(toWrite);
                                        }
                                        errorCheck(toReturn, standardType);
                                        if (toReturn.type.type != "ERR" || toReturn.type.type != "ERR")
                                        {
                                            if (standardType.type.type == "REAL")
                                            {
                                                firstArrayIndex = firstNum;
                                                secondArrayIndex = secondNum;
                                                toReturn.type.type = "AREAL";
                                            }
                                            else if (standardType.type.type == "INT")
                                            {
                                                firstArrayIndex = firstNum;
                                                secondArrayIndex = secondNum;
                                                toReturn.type.type = "AINT";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        reportParserError("an int", next);
                                        toReturn.type.type = "ERR*";
                                        errorRecov(toReturn);
                                    }
                                }
                                else
                                {
                                    reportParserError("]", next);
                                    toReturn.type.type = "ERR*";
                                    errorRecov(toReturn);
                                }
                            }
                            else
                            {
                                reportParserError("an int", next);
                                toReturn.type.type = "ERR*";
                                errorRecov(toReturn);
                            }
                        }
                        else
                        {
                            reportParserError("..", next);
                            toReturn.type.type = "ERR*";
                            errorRecov(toReturn);
                        }
                    }
                    else
                    {
                        reportParserError("a number", next);
                        toReturn.type.type = "ERR*";
                        errorRecov(toReturn);
                    }
                }
                else
                {
                    reportParserError("[", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseStandardType()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "standard_type";
            toReturn.expected = "integer or real";
            toReturn.follows.Add(";");
            toReturn.follows.Add(")");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 14)
            {
                toReturn.productionNum = "5.1";
                toReturn.type.type = "INT";
                tokenQueue.Dequeue();
            }
            else if (next.tokenType == 15)
            {
                toReturn.productionNum = "5.2";
                toReturn.type.type = "REAL";
                tokenQueue.Dequeue();
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseSubprogramDecarations()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "subprogram_declarations";
            toReturn.expected = "procedure";
            toReturn.follows.Add("begin");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 16)
            {
                toReturn.productionNum = "6.1";
                Wrapper subprogramDeclaration = parseSubprogramDeclaration();
                errorCheck(toReturn, subprogramDeclaration);
                next = tokenQueue.Peek();
                if (next.tokenType == 8)
                {
                    tokenQueue.Dequeue();
                    Wrapper subprogramDeclarations = parseSubprogramDecarations();
                    errorCheck(toReturn, subprogramDeclarations);
                }
                else
                {
                    reportParserError(";", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else toReturn.productionNum = "6.2";
            return toReturn;
        }

        private Wrapper parseSubprogramDeclaration()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "subprogram_declaration";
            toReturn.expected = "procedure";
            toReturn.follows.Add(";");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 16)
            {
                toReturn.productionNum = "7.1";
                Wrapper subprogramHead = parseSubprogramHead();
                errorCheck(toReturn, subprogramHead);
                Wrapper subprogramDeclaration2 = parseSubprogramDeclaration2();
                errorCheck(toReturn, subprogramDeclaration2);
                greenNodeStack.Pop();
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
                greenNodeStack.Pop();
            }
            return toReturn;
        }

        private Wrapper parseSubprogramDeclaration2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "subprogram_declaration'";
            toReturn.expected = "var, procedure, or begin";
            toReturn.follows.Add(";");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 11)
            {
                toReturn.productionNum = "7.2";
                Wrapper declarations = parseDeclarations();
                errorCheck(toReturn, declarations);
                Wrapper subprogramDeclaration3 = parseSubprogramDeclaration3();
                errorCheck(toReturn, subprogramDeclaration3);
            }
            else if (next.tokenType == 16)
            {
                toReturn.productionNum = "7.3";
                Wrapper subprogramDeclarations = parseSubprogramDecarations();
                errorCheck(toReturn, subprogramDeclarations);
                Wrapper compoundStatement = parseCompoundStatement();
                errorCheck(toReturn, compoundStatement);
            }
            else if (next.tokenType == 17)
            {
                toReturn.productionNum = "7.4";
                Wrapper compoundStatement = parseCompoundStatement();
                errorCheck(toReturn, compoundStatement);
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }

            return toReturn;
        }

        private Wrapper parseSubprogramDeclaration3()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "subprogram_declaration''";
            toReturn.expected = "procedure or begin";
            toReturn.follows.Add(";");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 16)
            {
                toReturn.productionNum = "7.5";
                Wrapper declarations = parseSubprogramDecarations();
                errorCheck(toReturn, declarations);
                Wrapper compoundStatement = parseCompoundStatement();
                errorCheck(toReturn, compoundStatement);
            }
            else if (next.tokenType == 17)
            {
                toReturn.productionNum = "7.6";
                Wrapper compoundStatement = parseCompoundStatement();
                errorCheck(toReturn, compoundStatement);
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseSubprogramHead()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "subprogram_head";
            toReturn.expected = "procedure";
            toReturn.follows.Add("var");
            toReturn.follows.Add("procedure");
            toReturn.follows.Add("begin");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 16)
            {
                toReturn.productionNum = "8.1";
                tokenQueue.Dequeue();
                next = tokenQueue.Peek();
                if (next.tokenType == 7)
                {
                    string id = next.lexeme;
                    tokenQueue.Dequeue();
                    if (currLine != next.lineNum)
                    {
                        currLine = next.lineNum;
                    }
                    if (!addGreenNode(id, "PROCNAME"))
                    {
                        string toWrite = "\t" + next.lineNum.ToString() + ". Errored lexeme: " + next.lexeme + ".";
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                        toReturn.type.type = "ERR*";
                    }
                    Wrapper subprogramHead2 = parseSubprogramHead2();
                    errorCheck(toReturn, subprogramHead2);
                }
                else
                {
                    reportParserError("an id", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }

            return toReturn;
        }

        private Wrapper parseSubprogramHead2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "subprogram_head'";
            toReturn.expected = ": or (";
            toReturn.follows.Add("var");
            toReturn.follows.Add("procedure");
            toReturn.follows.Add("begin");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 9 && next.attribute == "(")
            {
                toReturn.productionNum = "8.2";
                Wrapper arguments = parseArguments();
                errorCheck(toReturn, arguments);
                next = tokenQueue.Peek();
                if (next.tokenType == 8)
                {
                    tokenQueue.Dequeue();
                }
                else
                {
                    reportParserError(";", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else if (next.tokenType == 8)
            {
                toReturn.productionNum = "8.3";
                tokenQueue.Dequeue();
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseArguments()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "parameter_list";
            toReturn.expected = "(";
            toReturn.follows.Add(";");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 9 && next.attribute == "(")
            {
                toReturn.productionNum = "9.1";
                tokenQueue.Dequeue();
                Wrapper paremeterList = parseParameterList();
                errorCheck(toReturn, paremeterList);
                next = tokenQueue.Peek();
                if (next.tokenType == 9 && next.attribute == ")")
                {
                    tokenQueue.Dequeue();
                }
                else
                {
                    reportParserError(")", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }

            return toReturn;
        }

        private Wrapper parseParameterList()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "parameter_list";
            toReturn.expected = "an id";
            toReturn.follows.Add(")");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 7)
            {
                toReturn.productionNum = "10.1";
                string id = next.lexeme;
                tokenQueue.Dequeue();
                next = tokenQueue.Peek();
                if (next.tokenType == 9 && next.attribute == ":")
                {
                    tokenQueue.Dequeue();
                    Wrapper type = parseType();
                    if (type.type.type == "INT")
                    {
                        addBlueNode(id, "PPINT");
                    }
                    else if (type.type.type == "REAL")
                    {
                        addBlueNode(id, "PPREAL");
                    }
                    else if (type.type.type == "AINT")
                    {
                        addBlueNode(id, "PPAINT");
                    }
                    else if (type.type.type == "AREAL")
                    {
                        addBlueNode(id, "PPAREAL");
                    }
                    else
                    {
                        addBlueNode(id, "ERR");
                    }
                    errorCheck(toReturn, type);
                    Wrapper parameterList2 = parseParameterList2();
                    errorCheck(toReturn, parameterList2);
                }
                else
                {
                    reportParserError(":", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseParameterList2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "parameter_list'";
            toReturn.expected = ";";
            toReturn.follows.Add(")");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 8)
            {
                toReturn.productionNum = "10.2";
                tokenQueue.Dequeue();
                next = tokenQueue.Peek();
                if (next.tokenType == 7)
                {
                    string id = next.lexeme;
                    tokenQueue.Dequeue();
                    next = tokenQueue.Peek();
                    if (next.tokenType == 9 && next.attribute == ":")
                    {
                        tokenQueue.Dequeue();
                        Wrapper type = parseType();
                        errorCheck(toReturn, type);
                        if (type.type.type == "INT")
                        {
                            addBlueNode(id, "PPINT");
                        }
                        else if (type.type.type == "REAL")
                        {
                            addBlueNode(id, "PPREAL");
                        }
                        else if (type.type.type == "AINT")
                        {
                            addBlueNode(id, "PPAINT");
                        }
                        else if (type.type.type == "AREAL")
                        {
                            addBlueNode(id, "PPAREAL");
                        }
                        else
                        {
                            addBlueNode(id, "ERR");
                        }
                        Wrapper parameterList2 = parseParameterList2();
                        errorCheck(toReturn, parameterList2);
                    }
                    else
                    {
                        reportParserError(":", next);
                        toReturn.type.type = "ERR*";
                        errorRecov(toReturn);
                    }
                }
                else
                {
                    reportParserError("an id", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else toReturn.productionNum = "10.3";
            return toReturn;
        }

        private Wrapper parseCompoundStatement()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "compound_statement";
            toReturn.expected = "begin";
            toReturn.follows.Add("end");
            toReturn.follows.Add(".");
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 17)
            {
                toReturn.productionNum = "11.1";
                tokenQueue.Dequeue();
                Wrapper compoundStatement2 = parseCompoundStatement2();
                errorCheck(toReturn, compoundStatement2);
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseCompoundStatement2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "compound_statement'";
            toReturn.expected = "end, begin, while, if, an id, or call";
            toReturn.follows.Add("end");
            toReturn.follows.Add(".");
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 17 || next.tokenType == 22 || next.tokenType == 19 || next.tokenType == 7 || next.tokenType == 27)
            {
                toReturn.productionNum = "11.2";
                Wrapper optionalStatements = parseOptionalStatements();
                errorCheck(toReturn, optionalStatements);
                next = tokenQueue.Peek();
                if (next.tokenType == 18)
                {
                    tokenQueue.Dequeue();
                }
                else
                {
                    reportParserError("end", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else if (next.tokenType == 18)
            {
                toReturn.productionNum = "11.3";
                tokenQueue.Dequeue();
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseOptionalStatements()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "optional_statements";
            toReturn.expected = "begin, while, if, an id, or call";
            toReturn.follows.Add("end");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 17 || next.tokenType == 22 || next.tokenType == 19 || next.tokenType == 7 || next.tokenType == 27)
            {
                toReturn.productionNum = "12.1";
                Wrapper statementList = parseStatementList();
                errorCheck(toReturn, statementList);
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseStatementList()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "statement_list";
            toReturn.expected = "begin, while, if, an id, or call";
            toReturn.follows.Add("end");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 17 || next.tokenType == 22 || next.tokenType == 19 || next.tokenType == 7 || next.tokenType == 27)
            {
                toReturn.productionNum = "13.1";
                Wrapper statement = parseStatement();
                errorCheck(toReturn, statement);
                Wrapper statementList2 = parseStatementList2();
                errorCheck(toReturn, statementList2);
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseStatementList2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "statement_list'";
            toReturn.expected = ";";
            toReturn.follows.Add("end");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 8)
            {
                toReturn.productionNum = "13.2";
                tokenQueue.Dequeue();
                Wrapper statement = parseStatement();
                errorCheck(toReturn, statement);
                Wrapper statementList2 = parseStatementList2();
                errorCheck(toReturn, statementList2);
            }
            else toReturn.productionNum = "13.3";
            return toReturn;
        }

        private Wrapper parseStatement()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "statement";
            toReturn.expected = "begin, while, if, an id, or call";
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 17)//14.3 
            {
                toReturn.productionNum = "14.3";
                Wrapper compoundStatement = parseCompoundStatement();
                errorCheck(toReturn, compoundStatement);
            }
            else if (next.tokenType == 22)// 14.4
            {
                toReturn.productionNum = "14.4";
                tokenQueue.Dequeue();
                Wrapper expression = parseExpression();
                if (expression.type.type != "BOOL" && (expression.type.type != "ERR" || expression.type.type != "ERR*"))
                {
                    toReturn.type.type = "ERR*";
                    string toWrite = " Semantic error on Line " + next.lineNum + ": Conditional Expression Error";
                    Console.WriteLine(toWrite);
                    if (next.lineNum != currLine)
                    {
                        currLine = next.lineNum;
                    }
                    fileWrite(toWrite);
                }
                next = tokenQueue.Peek();
                if (next.tokenType == 23)
                {
                    tokenQueue.Dequeue();
                    Wrapper statement = parseStatement();
                    errorCheck(toReturn, statement);
                }
                else
                {
                    reportParserError("do", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else if (next.tokenType == 19)//14.5
            {
                toReturn.productionNum = "14.5";
                tokenQueue.Dequeue();
                Wrapper expression = parseExpression();
                if (expression.type.type != "BOOL" && expression.type.type != "ERR" && expression.type.type != "ERR*")
                {
                    toReturn.type.type = "ERR*";
                    string toWrite = " Semantic error on Line " + next.lineNum + ": Conditional Expression Error";
                    Console.WriteLine(toWrite);
                    if (next.lineNum != currLine)
                    {
                        currLine = next.lineNum;
                    }
                    fileWrite(toWrite);
                }
                errorCheck(toReturn, expression);
                next = tokenQueue.Peek();
                if (next.tokenType == 20)
                {
                    tokenQueue.Dequeue();
                    Wrapper statement = parseStatement();
                    errorCheck(toReturn, statement);
                    Wrapper statement2 = parseStatement2();
                    errorCheck(toReturn, statement2);
                }
                else
                {
                    reportParserError("then", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else if (next.tokenType == 7) //14.1
            {
                toReturn.productionNum = "14.1";
                Wrapper variable = parseVariable();
                errorCheck(toReturn, variable);
                next = tokenQueue.Peek();
                if (next.tokenType == Global_Vars.relopTokenType && next.attribute == "assignop")
                {
                    tokenQueue.Dequeue();
                    Wrapper expression = parseExpression();
                    errorCheck(toReturn, expression);
                    if (variable.type.type != "ERR" && variable.type.type != "ERR*" && variable.type.type != expression.type.type && 
                        expression.type.type != "ERR" && expression.type.type != "ERR*")
                    {
                        string toWrite = "Semantic Error on Line " + next.lineNum + ": Operand Type Mismatch";
                        Console.WriteLine(toWrite);
                        if (next.lineNum != currLine)
                        {
                            currLine = next.lineNum;
                        }
                        fileWrite(toWrite);
                        toReturn.type.type = "ERR*";
                    }
                }
                else
                {
                    reportParserError("assignop", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else if (next.tokenType == 27)//14.2
            {
                toReturn.productionNum = "14.2";
                Wrapper procedureStatement = parseProcedureStatement();
                errorCheck(toReturn, procedureStatement);
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }

            return toReturn;
        }

        private Wrapper parseStatement2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "statement'";
            toReturn.expected = "else";
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 21)
            {
                tokenQueue.Dequeue();
                toReturn.productionNum = "14.6";
                Wrapper statement = parseStatement();
                errorCheck(toReturn, statement);
            }
            else
            {
                toReturn.productionNum = "14.7";
            }

            return toReturn;
        }

        private Wrapper parseVariable()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "variable";
            toReturn.expected = "[";
            toReturn.follows.Add("assignop");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 7)
            {
                currentId = next.lexeme;
                if (next.lineNum != currLine)
                {
                    currLine = next.lineNum;
                }
                blueNode node = getBlueNode(currentId);
                if (node == null)
                {
                    string toWrite = "\t" + next.lineNum.ToString() + ". Errored lexeme: " + next.lexeme + ".";
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                    toReturn.type.type = "ERR*";
                }
                toReturn.productionNum = "15.1";
                tokenQueue.Dequeue();
                Wrapper variable2 = parseVariable2();
                errorCheck(toReturn, variable2);
                currentId = "";
                toReturn.type.type = variable2.type.type;
                if (node == null)
                {
                    Console.WriteLine(next.lineNum);
                    toReturn.type.type = "ERR*";
                }
                if (toReturn.type.type == "null")
                {
                    toReturn.type.type = node.type.type;
                }
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseVariable2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "variable'";
            toReturn.expected = "[";
            toReturn.follows.Add("assignop");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 9 && next.attribute == "[")
            {
                toReturn.productionNum = "15.2";
                tokenQueue.Dequeue();
                string idHold = currentId;
                Wrapper expression = parseExpression();
                errorCheck(toReturn, expression);
                next = tokenQueue.Peek();
                if (next.tokenType == 9 && next.attribute == "]")
                {
                    tokenQueue.Dequeue();
                    if (next.lineNum != currLine)
                    {
                        currLine = next.lineNum;
                    }
                    blueNode node = getBlueNode(idHold);
                    if (node == null)
                    {
                        string toWrite = "\t" + next.lineNum.ToString() + ". Errored lexeme: " + next.lexeme + ".";
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                        toReturn.type.type = "ERR*";
                    }
                    else if (expression.type.type == "INT")
                    {
                        if (node.type.type == "AINT" || node.type.type == "PPAINT")
                        {
                            toReturn.type.type = "INT";
                        }
                        else if (node.type.type == "AREAL" || node.type.type == "PPAREAL")
                        {
                            toReturn.type.type = "REAL";
                        }
                        else if (node.type.type == "ERR" || node.type.type == "ERR*")
                        {
                            toReturn.type.type = "ERR";
                        }
                        else
                        {
                            string toWrite = " Semantic error on Line " + next.lineNum + ": Variable is not an array";
                            Console.WriteLine(toWrite);
                            toReturn.type.type = "ERR*";
                            if (next.lineNum != currLine)
                            {
                                currLine = next.lineNum;
                            }
                            fileWrite(toWrite);
                        }
                    }
                    else
                    {
                        string toWrite = "Semantic Error on Line " + next.lineNum + ": Array index must be an integer";
                        Console.WriteLine(toWrite);
                        toReturn.type.type = "ERR*";
                        if (next.lineNum != currLine)
                        {
                            currLine = next.lineNum;
                        }
                        fileWrite(toWrite);
                    }
                }
                else
                {
                    reportParserError(toReturn.expected, next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                toReturn.productionNum = "15.3";
            }
            return toReturn;
        }

        private Wrapper parseProcedureStatement()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "procedure_statement";
            toReturn.expected = "call";
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 27)
            {
                toReturn.productionNum = "16.1";
                tokenQueue.Dequeue();
                next = tokenQueue.Peek();
                if (next.tokenType == 7)
                {
                    currentId = next.lexeme;
                    tokenQueue.Dequeue();
                    Wrapper procedureStatement2 = parseProcedureStatement2();
                    errorCheck(toReturn, procedureStatement2);
                    currentId = "";
                }
                else
                {
                    reportParserError("an id", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseProcedureStatement2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "procedure_statement'";
            toReturn.expected = "(";
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 9 && next.attribute == "(")
            {
                toReturn.productionNum = "16.2";
                tokenQueue.Dequeue();
                Wrapper expressionList = parseExpressionList();
                errorCheck(toReturn, expressionList);
                next = tokenQueue.Peek();
                if (next.tokenType == 9 && next.attribute == ")")
                {
                    tokenQueue.Dequeue();
                }
                else
                {
                    reportParserError(toReturn.expected, next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else 
            { 
                toReturn.productionNum = "16.3";
                string hold = currentId;
                if (next.lineNum != currLine)
                {
                    currLine = next.lineNum;
                }
                greenNode node = getGreenNode(hold);
                if (node == null)
                {
                    string toWrite = "\t" + next.lineNum.ToString() + ". Errored lexeme: " + next.lexeme + ".";
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                    toReturn.type.type = "ERR*";
                }
                if (currentParam < node.parameters.Count)
                {
                    toReturn.type.type = "ERR*";
                    string toWrite = "Semantic Error on Line " + next.lineNum + ": Too few parameters given for the procedure";
                    if (next.lineNum != currLine)
                    {
                        currLine = next.lineNum;
                    }
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                }
            }
            return toReturn;
        }

        private Wrapper parseExpressionList()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "expression_list";
            toReturn.expected = ",";
            toReturn.follows.Add(")");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 7 || next.tokenType == 24 || //17.1
                next.tokenType == Global_Vars.intTokenType || next.tokenType == Global_Vars.realTokenType || next.tokenType == Global_Vars.longrealTokenType ||
                (next.tokenType == 3 && (next.lexeme == "add" || next.lexeme == "subtract")))
            {
                toReturn.productionNum = "17.1";
                string hold = currentId;
                if (next.lineNum != currLine)
                {
                    currLine = next.lineNum;
                }
                greenNode node = getGreenNode(hold);
                if (node == null)
                {
                    string toWrite = "\t" + next.lineNum.ToString() + ". Errored lexeme: " + next.lexeme + ".";
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                    toReturn.type.type = "ERR*";
                }
                Wrapper expression = parseExpression();
                currentId = hold;
                errorCheck(toReturn, expression);
                if (node == null)
                {
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
                else if (currentParam + 1 > node.parameters.Count)
                {
                    toReturn.type.type = "ERR*";
                    string toWrite = "Semantic Error on Line " + next.lineNum + ": Too many parameters given for the procedure";
                    if (next.lineNum != currLine)
                    {
                        currLine = next.lineNum;
                    }
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                    errorRecov(toReturn);
                }
                else
                {
                    if (expression.type.type != node.parameters.ElementAt(currentParam).type.type)
                    {
                        string toWrite = "Semantic Error on Line " + next.lineNum + ": Parameter Type Mismatch";
                        if (next.lineNum != currLine)
                        {
                            currLine = next.lineNum;
                        }
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                    }
                    currentParam++;
                    Wrapper expressionList2 = parseExpressionList2();
                    errorCheck(toReturn, expressionList2);
                    if (currentParam < node.parameters.Count)
                    {
                        toReturn.type.type = "ERR*";
                        string toWrite = "Semantic Error on Line " + next.lineNum + ": Too few parameters given for the procedure";
                        if (next.lineNum != currLine)
                        {
                            currLine = next.lineNum;
                        }
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                    }
                    currentParam = 0;
                }
                
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        

        private Wrapper parseExpressionList2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "expression_list'";
            toReturn.expected = ",";
            toReturn.follows.Add(")");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 9 && next.attribute == ",")
            {
                toReturn.productionNum = "17.2";
                if (next.lineNum != currLine)
                {
                    currLine = next.lineNum;
                }
                greenNode node = getGreenNode(currentId);
                if (node == null)
                {
                    string toWrite = "\t" + next.lineNum.ToString() + ". Errored lexeme: " + next.lexeme + ".";
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                    toReturn.type.type = "ERR*";
                }
                string idHold = currentId;
                tokenQueue.Dequeue();
                Wrapper expression = parseExpression();
                errorCheck(toReturn, expression);
                if (node == null)
                {
                    toReturn.type.type = "ERR*";
                }
                else if (currentParam + 1 > node.parameters.Count)
                {
                    toReturn.type.type = "ERR*";
                    string toWrite = "Semantic Error on Line " + next.lineNum + ": Too many parameters given for the procedure";
                    if (next.lineNum != currLine)
                    {
                        currLine = next.lineNum;
                    }
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);

                }
                else
                {
                    if (expression.type.type != node.parameters.ElementAt(currentParam).type.type)
                    {
                        string toWrite = "Semantic Error on Line " + next.lineNum + ": Parameter Type Mismatch";
                        if (next.lineNum != currLine)
                        {
                            currLine = next.lineNum;
                        }
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                        toReturn.type.type = "ERR*";
                    }
                    currentParam++;
                    currentId = idHold;
                    Wrapper expressionList2 = parseExpressionList2();
                    errorCheck(toReturn, expressionList2);
                }
            }
            else toReturn.productionNum = "17.3";
            return toReturn;
        }

        private Wrapper parseExpression()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "expression";
            toReturn.expected = "a relop";
            toReturn.follows.Add("]");
            toReturn.follows.Add(",");
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 7 || next.tokenType == 24 || //18.1
                next.tokenType == Global_Vars.intTokenType || next.tokenType == Global_Vars.realTokenType || next.tokenType == Global_Vars.longrealTokenType ||
                (next.tokenType == 9 && next.attribute == "(") ||
                (next.tokenType == 3 && (next.lexeme == "add" || next.lexeme == "subtract")))
            {
                toReturn.productionNum = "18.1";
                Wrapper simpleExpression = parseSimpleExpression();
                errorCheck(toReturn, simpleExpression);
                leftType = simpleExpression.type;
                Wrapper expression2 = parseExpression2();
                errorCheck(toReturn, expression2);
                toReturn.type.type = expression2.type.type;
                leftType = null;
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }

            return toReturn;
        }

        private Wrapper parseExpression2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "expression'";
            toReturn.expected = "a relop";
            toReturn.follows.Add("]");
            toReturn.follows.Add(",");
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            Token next = tokenQueue.Peek();
            Type typeHold = leftType;
            if (next.tokenType == Global_Vars.relopTokenType)
            {
                tokenQueue.Dequeue();
                toReturn.productionNum = "18.2";
                Wrapper simpleExpression = parseSimpleExpression();
                errorCheck(toReturn, simpleExpression);
                if (toReturn.type.type == "ERR")
                {
                }
                else if (typeHold.type == simpleExpression.type.type)
                {
                    if (typeHold.type == "INT" || typeHold.type == "REAL")
                    {
                        toReturn.type.type = "BOOL";
                    }

                    else
                    {
                        string toWrite = "Semantic Error on Line " + next.lineNum + ": Parameter Type Mismatch";
                        if (next.lineNum != currLine)
                        {
                            currLine = next.lineNum;
                        }
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                        toReturn.type.type = "ERR*";
                    }
                }
                else
                {
                    string toWrite = "Semantic Error on Line " + next.lineNum + ": Operand Type Mismatch";
                    if (next.lineNum != currLine)
                    {
                        currLine = next.lineNum;
                    }
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                    toReturn.type.type = "ERR*";
                }
            }
            else
            {
                toReturn.productionNum = "18.3"; //epsilon production
                toReturn.type.type = typeHold.type;
            }
            return toReturn;
        }
        private Wrapper parseSimpleExpression()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "simple_expression";
            toReturn.expected = "a sign, id, or number";
            toReturn.follows.Add("]");
            toReturn.follows.Add(",");
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            toReturn.follows.Add("relop");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 7 || next.tokenType == 24 || //19.1
                (next.tokenType == 9 && next.attribute == "(") || 
                next.tokenType == Global_Vars.intTokenType || next.tokenType == Global_Vars.realTokenType || next.tokenType == Global_Vars.longrealTokenType)//if the next ID is a number
            {
                Wrapper term = parseTerm();
                leftType = term.type;
                errorCheck(toReturn, term);
                Wrapper simpleExpression2 = parseSimpleExpression2();
                errorCheck(toReturn, simpleExpression2);
                if(simpleExpression2.type.type == "null") toReturn.type = term.type;
                else toReturn.type = simpleExpression2.type;
            }
            else if (next.tokenType == 3 && (next.lexeme == "add" || next.lexeme == "subtract"))//19.2
            {
                Wrapper sign = parseSign();
                errorCheck(toReturn, sign);
                Wrapper term = parseTerm();
                errorCheck(toReturn, term);
                leftType = term.type;
                if (term.type.type == "INT" || term.type.type == "REAL")
                {
                    Wrapper simpleExpression2 = parseSimpleExpression2();
                    errorCheck(toReturn, simpleExpression2);
                    if(simpleExpression2.type.type == "null") toReturn.type = term.type;
                    else toReturn.type = simpleExpression2.type;
                }
                else
                {
                    if (term.type.type != "ERR" && term.type.type != "ERR*")
                    {
                       
                        string toWrite = "Semantic Error on Line " + next.lineNum + ": sign must be used with an int or real";
                        if (next.lineNum != currLine)
                        {
                            currLine = next.lineNum;
                        }
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                    }
                    toReturn.type.type = "ERR*";
                }
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            leftType = null;
            return toReturn;
        }

        private Wrapper parseSimpleExpression2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "simple_expression'";
            toReturn.expected = "addop";
            toReturn.follows.Add("]");
            toReturn.follows.Add(",");
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            toReturn.follows.Add("relop");
            Token next = tokenQueue.Peek();
            if (next.tokenType == Global_Vars.addopTokenType)//19.3
            {
                toReturn.productionNum = "19.3";
                //if (next.tokenType == 26)//OR operation, boolean operation
                //{
                //    tokenQueue.Dequeue();
                //    Wrapper term = parseTerm();
                //    errorCheck(toReturn, term);
                //    Wrapper simpleExpression = parseSimpleExpression2();
                //    errorCheck(toReturn, simpleExpression);
                //}
                if (next.tokenType == Global_Vars.addopTokenType)
                {
                    if (next.lexeme == "or")
                    {
                        tokenQueue.Dequeue();
                        Type holdType = leftType;
                        Wrapper term = parseTerm();
                        errorCheck(toReturn, term);
                        if (term.type.type != holdType.type)
                        {
                            toReturn.type.type = "ERR*";
                            Console.WriteLine("Semantic Error at line " + next.lineNum + " with lexeme " + next.lexeme +
                                ": attempted mixed-mode operation");
                        }
                        else if (term.type.type != "BOOL" && toReturn.type.type != "ERR")
                        {
                            toReturn.type.type = "ERR*";
                            string toWrite = "Semantic Error at line " + next.lineNum + " with lexeme " + next.lexeme +
                                ": attempted boolean operation with non-boolean value";
                            if (next.lineNum != currLine)
                            {
                                currLine = next.lineNum;
                            }
                            fileWrite(toWrite);
                            Console.WriteLine(toWrite);
                        }
                        else
                        {
                            leftType = term.type;
                            Wrapper simpleExpression = parseSimpleExpression2();
                            errorCheck(toReturn, simpleExpression);
                            toReturn.type = simpleExpression.type;
                            leftType = null;
                        }
                    }
                    else
                    {
                        tokenQueue.Dequeue();
                        Type holdType = leftType;
                        Wrapper term = parseTerm();
                        errorCheck(toReturn, term);
                        if (term.type.type != holdType.type && toReturn.type.type != "ERR")
                        {
                            toReturn.type.type = "ERR*";
                            string toWrite = "Semantic Error at line " + next.lineNum + " with lexeme " + next.lexeme +
                                ": attempted mixed-mode operation";
                            if (next.lineNum != currLine)
                            {
                                currLine = next.lineNum;
                            }
                            fileWrite(toWrite);
                            Console.WriteLine(toWrite);
                        }
                        else if (term.type.type == "BOOL" && toReturn.type.type != "ERR")
                        {
                            toReturn.type.type = "ERR*";
                            string toWrite = "Semantic Error at line " + next.lineNum + " with lexeme " + next.lexeme +
                                ": attempted boolean operation with non-boolean value";
                            if (next.lineNum != currLine)
                            {
                                currLine = next.lineNum;
                            }
                            fileWrite(toWrite);
                            Console.WriteLine(toWrite);
                        }
                        else
                        {
                            leftType = term.type;
                            Wrapper simpleExpression = parseSimpleExpression2();
                            errorCheck(toReturn, simpleExpression);
                            toReturn.type = simpleExpression.type;
                            leftType = null;
                        }
                    }
                }
            }
            else toReturn.productionNum = "19.4";
            return toReturn;
        }

        private Wrapper parseTerm()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "term";
            toReturn.expected = "a mulop, an id, or a num";
            toReturn.follows.Add("addop");
            toReturn.follows.Add("]");
            toReturn.follows.Add(",");
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            toReturn.follows.Add("relop");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 7 || next.tokenType == 24 || // If the next token is an ID or NOT
                (next.tokenType == 9 && next.attribute == "(") || 
                next.tokenType == Global_Vars.intTokenType || next.tokenType == Global_Vars.realTokenType || next.tokenType == Global_Vars.longrealTokenType)//if the next ID is a number
            {
                toReturn.productionNum = "20.1";
                currentId = next.lexeme;
                Wrapper factor = parseFactor();
                errorCheck(toReturn, factor);
                leftType = factor.type;
                Wrapper term2 = parseTerm2();
                errorCheck(toReturn, term2);
                toReturn.type = term2.type;
                if (toReturn.type.type == "null") toReturn.type = factor.type; // term2 was epsilon, set type = to factor
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseTerm2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "term'";
            toReturn.expected = "a mulop";
            toReturn.follows.Add("addop");
            toReturn.follows.Add("]");
            toReturn.follows.Add(",");
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            toReturn.follows.Add("relop");
            Token next = tokenQueue.Peek();
            if (next.tokenType == Global_Vars.mulopTokenType)// 20.2
            {
                tokenQueue.Dequeue();
                toReturn.productionNum = "20.2";
                //if (next.tokenType == 25)//AND operation, requires boolean.
                //{
                //    //TO BE ADDED
                //    Wrapper factor = parseFactor();
                //    errorCheck(toReturn, factor);
                //    Wrapper term2 = parseTerm2();
                //    errorCheck(toReturn, term2);
                //}
                if (true)//Numerical operation, requires numbers.
                {
                    Type holdType = leftType;
                    if (next.lexeme == "and")
                    {
                        Wrapper factor = parseFactor();
                        errorCheck(toReturn, factor);
                        if (factor.type.type != holdType.type && toReturn.type.type != "ERR")
                        {
                            toReturn.type.type = "ERR*";
                            string toWrite = "Semantic Error at line " + next.lineNum + " with lexeme " + next.lexeme +
                                ": attempted mixed-mode operation";
                            if (next.lineNum != currLine)
                            {
                                currLine = next.lineNum;
                            }
                            fileWrite(toWrite);
                            Console.WriteLine(toWrite);
                        }
                        else if (factor.type.type != "BOOL" && toReturn.type.type != "ERR")
                        {
                            toReturn.type.type = "ERR*";
                            string toWrite = "Semantic Error at line " + next.lineNum + " with lexeme " + next.lexeme +
                                ": attempted boolean operation with non-boolean value";
                            if (next.lineNum != currLine)
                            {
                                currLine = next.lineNum;
                            }
                            fileWrite(toWrite);
                            Console.WriteLine(toWrite);
                        }
                        else
                        {
                            leftType = factor.type;
                            Wrapper term2 = parseTerm2();
                            errorCheck(toReturn, term2);
                        }
                    }
                    else
                    {
                        Wrapper factor = parseFactor();
                        errorCheck(toReturn, factor);
                        if (factor.type.type != holdType.type && toReturn.type.type != "ERR")
                        {
                            toReturn.type.type = "ERR*";
                            string toWrite = "Semantic Error at line " + next.lineNum + " with lexeme " + next.lexeme +
                                ": attempted mixed-moed operation";
                            if (next.lineNum != currLine)
                            {
                                currLine = next.lineNum;
                            }
                            fileWrite(toWrite);
                            Console.WriteLine(toWrite);
                        }
                        else if (factor.type.type == "BOOL" && toReturn.type.type != "ERR")
                        {
                            toReturn.type.type = "ERR*";
                            string toWrite = "Semantic Error at line " + next.lineNum + " with lexeme " + next.lexeme +
                                ": attempted non-boolean operation with boolean value";
                            if (next.lineNum != currLine)
                            {
                                currLine = next.lineNum;
                            }
                            fileWrite(toWrite);
                            Console.WriteLine(toWrite);
                        }
                        else if (next.lexeme == "mod" && holdType.type != "INT" && toReturn.type.type != "ERR")
                        {
                            toReturn.type.type = "ERR*";
                            Console.WriteLine("Error at line " + next.lineNum + " with lexeme " + next.lexeme +
                                ": Types do not match operation");
                            string toWrite = "Semantic Error at line " + next.lineNum + " with lexeme " + next.lexeme +
                                ": Types do not match operation";
                            if (next.lineNum != currLine)
                            {
                                currLine = next.lineNum;
                            }
                            fileWrite(toWrite);
                            Console.WriteLine(toWrite);
                        }
                        else
                        {
                            leftType = factor.type;
                            Wrapper term2 = parseTerm2();
                            errorCheck(toReturn, term2);
                        }
                    }
                }
            }
            else
            {
                toReturn.productionNum = "20.3";
                toReturn.type = leftType;
            }
            leftType = null;
            return toReturn;
        }

        private Wrapper parseFactor()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "factor";
            toReturn.expected = "an id, number, or not";
            toReturn.follows.Add("mulop");
            toReturn.follows.Add("addop");
            toReturn.follows.Add("]");
            toReturn.follows.Add(",");
            toReturn.follows.Add("do");
            toReturn.follows.Add("then");
            toReturn.follows.Add(")");
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            toReturn.follows.Add("relop");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 7)//21.1
            {
                toReturn.productionNum = "21.1";
                tokenQueue.Dequeue();
                doubleCheck = true;
                if (next.lineNum != currLine)
                {
                    currLine = next.lineNum;
                }
                currentBlueNode = getBlueNode(next.lexeme);
                if(currentBlueNode == null){
                    currentGreenNode = getGreenNode(next.lexeme);
                    if (currentGreenNode == null)
                    {
                        string toWrite = "\t" + next.lineNum.ToString() + ". Errored lexeme: " + next.lexeme + ".";
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                        toReturn.type.type = "ERR*";
                        errorRecov(toReturn);
                        doubleCheck = false;
                        return toReturn;
                    }
                }
                else leftType = currentBlueNode.type;
                doubleCheck = false;
                Wrapper factor2 = parseFactor2();
                errorCheck(toReturn, factor2);
                toReturn.type = factor2.type;
            }
            else if (next.tokenType == Global_Vars.intTokenType || next.tokenType == Global_Vars.realTokenType || next.tokenType == Global_Vars.longrealTokenType)
            //21.2
            {
                tokenQueue.Dequeue();
                toReturn.productionNum = "21.2";
                if (next.tokenType == Global_Vars.intTokenType) toReturn.type.type = "INT";
                else if (next.tokenType == Global_Vars.realTokenType) toReturn.type.type = "REAL";
                else if (next.tokenType == Global_Vars.longrealTokenType) toReturn.type.type = "REAL";
            }
            else if (next.tokenType == 24)//21.3
            {
                toReturn.productionNum = "21.3";
                tokenQueue.Dequeue();
                Wrapper factor = parseFactor();
                errorCheck(toReturn, factor);
                if (factor.type.type == "BOOL")
                {
                    toReturn.type = factor.type;
                }
                else
                {
                    if (toReturn.type.type != "ERR")
                    {
                        toReturn.type.type = "ERR*";
                        string toWrite = "Semantic Error on Line " + next.lineNum + ", lexeme " + next.lexeme + ": attempting NOT operation with non-boolean value.";
                        if (next.lineNum != currLine)
                        {
                            currLine = next.lineNum;
                        }
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                    }
                }
            }
            else if (next.tokenType == 9 && next.attribute == "(")
            {
                tokenQueue.Dequeue();
                Wrapper expression = parseExpression();
                next = tokenQueue.Peek();
                if (next.tokenType == 9 && next.attribute == ")")
                {
                    toReturn.type = expression.type;
                    tokenQueue.Dequeue();
                }
                else
                {
                    reportParserError(")", next);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private Wrapper parseFactor2()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "factor'";
            toReturn.expected = "( or [";
            toReturn.follows.Add("mulop");
            toReturn.follows.Add("addop");
            toReturn.follows.Add("]");
            toReturn.follows.Add(",");
            toReturn.follows.Add("do");
            toReturn.follows.Add("then");
            toReturn.follows.Add(")");
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            toReturn.follows.Add("relop");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 9)
            {
                if (next.attribute == "(")// 21.5
                {
                    toReturn.productionNum = "21.5";
                    if (currentGreenNode == null)
                    {
                        toReturn.type.type = "ERR*";
                        string toWrite = "Semantic Error on Line " + next.lineNum + ", lexeme " + next.lexeme + ": Identifier is not a procedure";
                        if (next.lineNum != currLine)
                        {
                            currLine = next.lineNum;
                        }
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                        errorRecov(toReturn);
                        return toReturn;
                    }
                    tokenQueue.Dequeue();
                    currentParam = 0;
                    Wrapper expressionList = parseExpressionList();
                    errorCheck(toReturn, expressionList);
                    next = tokenQueue.Peek();
                    if (next.tokenType == 9 && next.attribute == ")")
                    {
                        tokenQueue.Dequeue();
                        toReturn.type.type = "PROC";
                        //Correct state, do nothing
                    }
                    else
                    {
                        reportParserError(toReturn.expected, next);
                        toReturn.type.type = "ERR*";
                        errorRecov(toReturn);
                    }

                }
                else if (next.attribute == "[")//21.6
                {
                    if (currentBlueNode == null || (currentBlueNode.type.type != "AREAL" && currentBlueNode.type.type != "AINT"))
                    {
                        toReturn.type.type = "ERR*";
                        Console.WriteLine("Error on line " + next.lineNum + ": Identifier is not an array.");
                        string toWrite = "Semantic Error on Line " + next.lineNum + ", lexeme " + next.lexeme + ": Identifier is not an array";
                        if (next.lineNum != currLine)
                        {
                            currLine = next.lineNum;
                        }
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                        errorRecov(toReturn);
                        return toReturn;
                    }
                    toReturn.productionNum = "21.5";
                    tokenQueue.Dequeue();
                    blueNode blueHold = currentBlueNode;
                    Wrapper expression = parseExpression();
                    errorCheck(toReturn, expression);
                    if (expression.type.type != "INT")
                    {
                        toReturn.type.type = "ERR*";
                        string toWrite = "Semantic Error on Line " + next.lineNum + ", lexeme " + next.lexeme + ": Array inicies must be integers";
                        if (next.lineNum != currLine)
                        {
                            currLine = next.lineNum;
                        }
                        fileWrite(toWrite);
                        Console.WriteLine(toWrite);
                        errorRecov(toReturn);
                        return toReturn;
                    }
                    next = tokenQueue.Peek();
                    if (next.tokenType == 9 && next.attribute == "]")
                    {
                        tokenQueue.Dequeue();
                        if (blueHold.type.type == "AINT") toReturn.type.type = "INT";
                        else if (blueHold.type.type == "AREAL") toReturn.type.type = "REAL";
                        //Correct state, do nothing
                    }
                    else
                    {
                        reportParserError(toReturn.expected, next);
                        toReturn.type.type = "ERR*";
                        errorRecov(toReturn);
                    }
                }
                else
                {
                    toReturn.type.type = currentBlueNode.type.type;
                }
            }
            else
            {
                toReturn.type.type = currentBlueNode.type.type;
            }
            return toReturn;
        }

        private Wrapper parseSign()
        {
            Wrapper toReturn = new Wrapper();
            toReturn.nonTerminal = "sign";
            toReturn.expected = "+ or -";
            toReturn.follows.Add("id");
            toReturn.follows.Add("num");
            toReturn.follows.Add("not");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 3 && (next.lexeme == "add" || next.lexeme == "subtract"))
            {
                tokenQueue.Dequeue();
                if (next.lexeme == "add") toReturn.type.type = "positive";
                else toReturn.type.type = "negative";
            }
            else
            {
                reportParserError(toReturn.expected, next);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
            return toReturn;
        }

        private void errorCheck(Wrapper parent, Wrapper child)
        {
            parent.children.Add(child);
            if (child.type.type == "ERR" || child.type.type == "ERR*")
            {
                parent.type.type = "ERR";
            }
        }

        private void errorRecov(Wrapper erroredWrapper)
        {
            if (tokenQueue.Count < 1)
            {
                Console.WriteLine("Ran out of tokens while trying to recover from error.");
                return;
            }
            
            Boolean foundFollows = false;
            while (!foundFollows)
            {
                Token next = tokenQueue.Peek();
                if (tokenQueue.Count < 1)
                {
                    Console.WriteLine("Ran out of tokens while trying to recover from error.");
                    return;
                }
                if (erroredWrapper.follows.Contains(next.lexeme) || erroredWrapper.follows.Contains(next.attribute))
                {
                    foundFollows = true;
                }
                else
                {
                    tokenQueue.Dequeue();
                }
            }
        }
        private bool addGreenNode(string id, string type)
        {
            greenNode newNode = new greenNode();
            newNode.id = id;
            newNode.type.type = type;
            if (greenNodeStack.Count < 1)
            {
                greenNodeStack.Push(newNode);
                return true;
            }
            greenNode currentTop = greenNodeStack.Peek();
            foreach (greenNode cur in currentTop.children)
            {
                if (cur.id == id)
                {
                    string toWrite = "Semantic Error: ID already exists in this scope. On line: ";
                    newNode.parent = greenNodeStack.Peek();
                    greenNodeStack.Push(newNode);
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                    return false;
                }
            }
            if (currentTop.id == id)
            {
                string toWrite = "Semantic Error: ID already exists in this scope. On line: ";
                newNode.parent = greenNodeStack.Peek();
                greenNodeStack.Push(newNode);
                fileWrite(toWrite);
                Console.WriteLine(toWrite);
                return false;
            }
            newNode.parent = greenNodeStack.Peek();
            newNode.parent.children.Add(newNode);
            greenNodeStack.Push(newNode);
            offset = 0;
            return true;
        }

        private bool addBlueNode(string id, string type)
        {
            blueNode newNode = new blueNode();
            newNode.type.type = type;
            newNode.id = id;
            greenNode top = greenNodeStack.Peek();
            foreach (blueNode var in top.vars)
            {
                if (var.id == id)
                {
                    string toWrite = "Semantic Error: ID already exists in this scope. On line: ";
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                    return false;
                }
            }
            foreach (blueNode param in top.parameters)
            {
                if (param.id == id)
                {
                    string toWrite = "Semantic Error: ID already exists in this scope. On line: ";
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                    return false;
                }
            }
            if (type == "PPINT" || type == "PPREAL" || type == "PPAINT" || type == "PPAREAL")
            {
                string newType = "";
                if (type == "PPINT") newType = "INT";
                else if (type == "PPREAL") newType = "REAL";
                else if (type == "PPAINT") newType = "AINT";
                else if (type == "PPAREAL") newType = "AREAL";
                else
                {
                    string toWrite = "Semantic Error: Error Parsing Parameter Types. On line: ";
                    fileWrite(toWrite);
                    Console.WriteLine(toWrite);
                }
                newNode.type.type = newType;
                top.parameters.Add(newNode);
                memAlloc(newNode);
                return true;
            }
            memAlloc(newNode);
            newNode.rightIndex = secondArrayIndex;
            newNode.leftIndex = firstArrayIndex;
            top.vars.Add(newNode);
            return true;
        }

        private void memAlloc(blueNode newNode)
        {
            if (newNode.type.type == "INT")
            {
                newNode.type.size = Global_Vars.Integer_Size;
                newNode.memLoc = offset;
                offset += newNode.type.size;
            }
            else if (newNode.type.type == "REAL")
            {
                newNode.type.size = Global_Vars.Real_Size;
                newNode.memLoc = offset;
                offset += newNode.type.size;
            }
            else if (newNode.type.type == "AREAL")
            {
                int multiplier = secondArrayIndex - firstArrayIndex + 1;
                int totalMemoryNeeded = multiplier * Global_Vars.Real_Size;
                newNode.type.size = totalMemoryNeeded;
                newNode.memLoc = offset;
                offset += totalMemoryNeeded;
            }
            else if (newNode.type.type == "AINT")
            {
                int multiplier = secondArrayIndex - firstArrayIndex + 1;
                int totalMemoryNeeded = multiplier * Global_Vars.Integer_Size;
                newNode.type.size = totalMemoryNeeded;
                newNode.memLoc = offset;
                offset += totalMemoryNeeded;
            }
            /*else if (newNode.type.type != "PGPARAM") 
            {
                Console.Out.WriteLine("Internal error calculating memory");
            }*/
        }

        private blueNode getBlueNode(string currentId)
        {
            greenNode top = greenNodeStack.Peek();
            blueNode toReturn = null;
            foreach (blueNode var in top.vars)
            {
                if (var.id == currentId)
                {
                    toReturn = var;
                }
            }
            if (toReturn == null)
            {
                foreach (blueNode var in top.parameters)
                {
                    if (var.id == currentId)
                    {
                        toReturn = var;
                        return toReturn;
                    }
                }
            }
            if (top.parent != null && toReturn == null)
            {
                toReturn = blueParentSearch(currentId, top);
            }
            if (toReturn == null && !doubleCheck)
            {
                string toWrite = "Semantic Error: variable not foun in current scope. On line: ";
                fileWrite(toWrite);
                Console.WriteLine(toWrite);
            }
            return toReturn;
        }

        private blueNode blueParentSearch(string currentId, greenNode input)
        {
            greenNode checking = input.parent;
            blueNode toReturn = null;
            foreach (blueNode var in checking.vars)
            {
                if (var.id == currentId)
                {
                    toReturn = var;
                }
            }
            if (checking.parent != null && toReturn == null)
            {
                toReturn = blueParentSearch(currentId, checking);
            }
            return toReturn;
        }

        private greenNode getGreenNode(string currentId)
        {
            greenNode top = greenNodeStack.Peek();
            greenNode toReturn = null;
            if (top.id == currentId)
            {
                toReturn = top;
                return toReturn;
            }
            foreach (greenNode var in top.children)
            {
                if (var.id == currentId)
                {
                    toReturn = var;
                }
            }
            if (top.parent != null && toReturn == null)
            {
                toReturn = greenParentSearch(currentId, top);
            }
            if (toReturn == null && !doubleCheck)
            {
 
                string toWrite = "Semantic Error: procedure not found in current scope. Looking for: " + currentId + ". On line: ";
                fileWrite(toWrite);
                Console.WriteLine(toWrite);
            }
            else if (toReturn == null && doubleCheck)
            {
                string toWrite = "Semantic Error: procedure/var not found in current scope. Looking for: " + currentId + ". On line: ";
                fileWrite(toWrite);
                Console.WriteLine(toWrite);
            }
            return toReturn;
        }

        private greenNode greenParentSearch(string currentId, greenNode input)
        {
            greenNode checking = input.parent;
            greenNode toReturn = null;
            if (checking.id == currentId)
            {
                toReturn = checking;
                return toReturn;
            }
            foreach (greenNode var in checking.children)
            {
                if (var.id == currentId)
                {
                    toReturn = var;
                }
            }
            if (checking.parent != null && toReturn == null)
            {
                toReturn = greenParentSearch(currentId, checking);
            }
            return toReturn;
        }

        private void outputMemories()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Global_Vars.memOutputFile))
            {
                greenNode program = greenNodeStack.Pop();
                file.WriteLine(program.id);
                foreach (blueNode var in program.parameters)
                {
                    file.WriteLine("\t" + var.makeString());
                }
                foreach (blueNode var in program.vars)
                {
                    file.WriteLine("\t" + var.makeString());
                }
                foreach (greenNode child in program.children)
                {
                    outputMemoryChain(child, file);
                }
            }
        }

        private void outputMemoryChain(greenNode node, System.IO.StreamWriter file)
        {
            file.WriteLine(node.id);
            foreach (blueNode var in node.parameters)
            {
                file.WriteLine("\t" + var.makeString());
            }
            foreach (blueNode var in node.vars)
            {
                file.WriteLine("\t" + var.makeString());
            }
            foreach (greenNode child in node.children)
            {
                outputMemoryChain(child, file);
            }
        }
    }
}
