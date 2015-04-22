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
        string currentId = "";
        int currentParam = 0;
        Stack<greenNode> greenNodeStack = new Stack<greenNode>();
        Type leftType = null;

        public void parse()
        {
           // Queue<Token> tokenQueue = Global_Vars.tokenQueue;

            parseProgram();
            
        }

        public void reportParserError(string expected, string actual, int line)
        {
            Console.WriteLine("Error on line " + line + ": Expected " + expected + ", got: " + actual + ".");
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
                            }
                            else
                            {
                                reportParserError(";", next.lexeme, next.lineNum);
                                toReturn.type.type = "ERR*";
                            }
                        }
                        else
                        {
                            reportParserError(")", next.lexeme, next.lineNum);
                            toReturn.type.type = "ERR*";
                        }
                    }
                    else
                    {
                        reportParserError("(", next.lexeme, next.lineNum);
                        toReturn.type.type = "ERR*";
                    }
                }
                else
                {
                    reportParserError("an id", next.lexeme, next.lineNum);
                    toReturn.type.type = "ERR*";
                }
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
                toReturn.type.type = "ERR*";
            }
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
                    reportParserError(".", next.lexeme, next.lineNum);
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
                    reportParserError(".", next.lexeme, next.lineNum);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                if (!addBlueNode(next.lexeme, "PGPARAM"))
                {
                    toReturn.type.type = "ERR*";
                }
                Wrapper identifierList2 = parseIdentifierList2();
                errorCheck(toReturn, identifierList2);
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                        if (!addBlueNode(next.lexeme, "PGPARAM"))
                        {
                            toReturn.type.type = "ERR*";
                        }
                        Wrapper identifierList2 = parseIdentifierList2();
                        errorCheck(toReturn, identifierList2);
                    }
                    else
                    {
                        reportParserError("an id", next.lexeme, next.lineNum);
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
                    reportParserError(toReturn.expected, next.lexeme, next.lineNum);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                        if (!addBlueNode(id, type.type.type))
                        {
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
                            reportParserError(";", next.lexeme, next.lineNum);
                            toReturn.type.type = "ERR*";
                            errorRecov(toReturn);
                        }
                    }
                    else
                    {
                        reportParserError(":", next.lexeme, next.lineNum);
                        toReturn.type.type = "ERR*";
                        errorRecov(toReturn);
                    }
                }
                else
                {
                    reportParserError("an id", next.lexeme, next.lineNum);
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                                            Console.WriteLine("error on line: " + next.lineNum + ", improper array indicies");
                                        }
                                        errorCheck(toReturn, standardType);
                                        if (toReturn.type.type != "ERR" || toReturn.type.type != "ERR")
                                        {
                                            if (standardType.type.type == "REAL")
                                            {
                                                toReturn.type.type = "AREAL";
                                            }
                                            else if (standardType.type.type == "INT")
                                            {
                                                toReturn.type.type = "AINT";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        reportParserError("an int", next.lexeme, next.lineNum);
                                        toReturn.type.type = "ERR*";
                                        errorRecov(toReturn);
                                    }
                                }
                                else
                                {
                                    reportParserError("]", next.lexeme, next.lineNum);
                                    toReturn.type.type = "ERR*";
                                    errorRecov(toReturn);
                                }
                            }
                            else
                            {
                                reportParserError("an int", next.lexeme, next.lineNum);
                                toReturn.type.type = "ERR*";
                                errorRecov(toReturn);
                            }
                        }
                        else
                        {
                            reportParserError("..", next.lexeme, next.lineNum);
                            toReturn.type.type = "ERR*";
                            errorRecov(toReturn);
                        }
                    }
                    else
                    {
                        reportParserError("a number", next.lexeme, next.lineNum);
                        toReturn.type.type = "ERR*";
                        errorRecov(toReturn);
                    }
                }
                else
                {
                    reportParserError("[", next.lexeme, next.lineNum);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                    reportParserError(";", next.lexeme, next.lineNum);
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
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                    if (!addGreenNode(id, "PROCNAME"))
                    {
                        toReturn.type.type = "ERR*";
                    }
                    Wrapper subprogramHead2 = parseSubprogramHead2();
                    errorCheck(toReturn, subprogramHead2);
                }
                else
                {
                    reportParserError("an id", next.lexeme, next.lineNum);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                    reportParserError(";", next.lexeme, next.lineNum);
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                    reportParserError(")", next.lexeme, next.lineNum);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                    reportParserError(":", next.lexeme, next.lineNum);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                        reportParserError(":", next.lexeme, next.lineNum);
                        toReturn.type.type = "ERR*";
                        errorRecov(toReturn);
                    }
                }
                else
                {
                    reportParserError("an id", next.lexeme, next.lineNum);
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                    reportParserError("end", next.lexeme, next.lineNum);
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                if (expression.type.type != "BOOL")
                {
                    toReturn.type.type = "ERR*";
                    Console.WriteLine("Line " + next.lineNum + ": Conditional Expression Error");
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
                    reportParserError("do", next.lexeme, next.lineNum);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else if (next.tokenType == 19)//14.5
            {
                toReturn.productionNum = "14.5";
                tokenQueue.Dequeue();
                Wrapper expression = parseExpression();
                if (expression.type.type != "BOOL")
                {
                    toReturn.type.type = "ERR*";
                    Console.WriteLine("Line " + next.lineNum + ": Conditional Expression Error");
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
                    reportParserError("then", next.lexeme, next.lineNum);
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
                    if (variable.type.type != "ERR" && variable.type.type != "ERR*" && variable.type.type != expression.type.type)
                    {
                        Console.WriteLine("Line " + next.lineNum + ": Operand Type Mismatch");
                        toReturn.type.type = "ERR*";
                    }
                }
                else
                {
                    reportParserError("assignop", next.lexeme, next.lineNum);
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                toReturn.productionNum = "15.1";
                tokenQueue.Dequeue();
                Wrapper variable2 = parseVariable2();
                errorCheck(toReturn, variable2);
                currentId = "";
                toReturn.type.type = variable2.type.type;
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                Wrapper expression = parseExpression();
                errorCheck(toReturn, expression);
                next = tokenQueue.Peek();
                if (next.tokenType == 9 && next.attribute == "]")
                {
                    tokenQueue.Dequeue();
                    blueNode node = getBlueNode(currentId);
                    if (node == null)
                    {
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
                            Console.WriteLine("Line " + next.lineNum + ": Variable is not an array");
                            toReturn.type.type = "ERR*";
                        }
                    }
                    else
                    {
                        Console.WriteLine("Line " + next.lineNum + ": Array index must be an integer");
                    }
                }
                else
                {
                    reportParserError(toReturn.expected, next.lexeme, next.lineNum);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else toReturn.productionNum = "15.3";
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
                    reportParserError("an id", next.lexeme, next.lineNum);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                    reportParserError(toReturn.expected, next.lexeme, next.lineNum);
                    toReturn.type.type = "ERR*";
                    errorRecov(toReturn);
                }
            }
            else toReturn.productionNum = "16.3";
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
                greenNode node = getGreenNode(currentId);
                Wrapper expression = parseExpression();
                errorCheck(toReturn, expression);
                if (node == null)
                {
                    toReturn.type.type = "ERR*";
                }
                else if (currentParam + 1 > node.parameters.Count)
                {
                    toReturn.type.type = "ERR*";
                    Console.WriteLine("Line " + next.lineNum + ": Too many parameters given for the procedure");
                }
                else
                {
                    if (expression.type.type != node.parameters.ElementAt(currentParam).type.type)
                    {
                        Console.WriteLine("Line " + next.lineNum + ": Parameter Type Mismatch");
                    }
                    currentParam++;
                    Wrapper expressionList2 = parseExpressionList2();
                    errorCheck(toReturn, expressionList2);
                    currentParam = 0;
                }
                
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
                greenNode node = getGreenNode(currentId);
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
                    Console.WriteLine("Line " + next.lineNum + ": Too many parameters given for the procedure");
                }
                else
                {
                    if (expression.type.type != node.parameters.ElementAt(currentParam).type.type)
                    {
                        Console.WriteLine("Line " + next.lineNum + ": Parameter Type Mismatch");
                        toReturn.type.type = "ERR*";
                    }
                    currentParam++;
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
            if (next.tokenType == Global_Vars.relopTokenType)
            {
                tokenQueue.Dequeue();
                toReturn.productionNum = "18.2";
                Wrapper simpleExpression = parseSimpleExpression();
                errorCheck(toReturn, simpleExpression);
                if (toReturn.type.type == "ERR")
                {
                }
                else if (leftType.type == simpleExpression.type.type)
                {
                    if (leftType.type == "INT" || leftType.type == "REAL")
                    {
                        toReturn.type.type = "BOOL";
                    }
                    
                    else
                    {
                        Console.WriteLine("Line " + next.lineNum + ": Parameter Type Mismatch");
                        toReturn.type.type = "ERR*";
                    }
                }
                else
                {
                    Console.WriteLine("Line " + next.lineNum + ": Operand Type Mismatch");
                    toReturn.type.type = "ERR*";
                }
            }
            else toReturn.productionNum = "18.3"; //epsilon production
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
                next.tokenType == Global_Vars.intTokenType || next.tokenType == Global_Vars.realTokenType || next.tokenType == Global_Vars.longrealTokenType)//if the next ID is a number
            {
                Wrapper term = parseTerm();
                errorCheck(toReturn, term);
                Wrapper simpleExpression2 = parseSimpleExpression2();
                errorCheck(toReturn, simpleExpression2);
            }
            else if (next.tokenType == 3 && (next.lexeme == "add" || next.lexeme == "subtract"))//19.2
            {
                Wrapper sign = parseSign();
                errorCheck(toReturn, sign);
                Wrapper term = parseTerm();
                errorCheck(toReturn, term);
                Wrapper simpleExpression2 = parseSimpleExpression2();
                errorCheck(toReturn, simpleExpression2);
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
                toReturn.type.type = "ERR*";
                errorRecov(toReturn);
            }
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
            if (next.tokenType == Global_Vars.addopTokenType || next.tokenType == 26)//19.3
            {
                toReturn.productionNum = "19.3";
                if (next.tokenType == 26)//OR operation, boolean operation
                {
                    tokenQueue.Dequeue();
                    Wrapper term = parseTerm();
                    errorCheck(toReturn, term);
                    Wrapper simpleExpression = parseSimpleExpression2();
                    errorCheck(toReturn, simpleExpression);
                }
                else if (next.tokenType == Global_Vars.addopTokenType)//numerical addop operation
                {
                    tokenQueue.Dequeue();
                    Wrapper term = parseTerm();
                    errorCheck(toReturn, term);
                    Wrapper simpleExpression = parseSimpleExpression2();
                    errorCheck(toReturn, simpleExpression);
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
                next.tokenType == Global_Vars.intTokenType || next.tokenType == Global_Vars.realTokenType || next.tokenType == Global_Vars.longrealTokenType)//if the next ID is a number
            {
                toReturn.productionNum = "20.1";
                Wrapper factor = parseFactor();
                errorCheck(toReturn, factor);
                Wrapper term2 = parseTerm2();
                errorCheck(toReturn, term2);
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
            if (next.tokenType == Global_Vars.mulopTokenType || next.tokenType == 25)// 20.2
            {
                tokenQueue.Dequeue();
                toReturn.productionNum = "20.2";
                if (next.tokenType == 25)//AND operation, requires boolean.
                {
                    //TO BE ADDED
                    Wrapper factor = parseFactor();
                    errorCheck(toReturn, factor);
                    Wrapper term2 = parseTerm2();
                    errorCheck(toReturn, term2);
                }
                else //Numerical operation, requires numbers.
                {
                    Wrapper factor = parseFactor();
                    errorCheck(toReturn, factor);
                    Wrapper term2 = parseTerm2();
                    errorCheck(toReturn, term2);
                }
            }
            else toReturn.productionNum = "20.3";
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
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            toReturn.follows.Add("relop");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 7)//21.1
            {
                toReturn.productionNum = "21.1";
                tokenQueue.Dequeue();
                Wrapper factor2 = parseFactor2();
                errorCheck(toReturn, factor2);
            }
            else if (next.tokenType == Global_Vars.intTokenType || next.tokenType == Global_Vars.realTokenType || next.tokenType == Global_Vars.longrealTokenType)
            //21.2
            {
                tokenQueue.Dequeue();
                toReturn.productionNum = "21.2";
                if (next.tokenType == Global_Vars.intTokenType) toReturn.type.type = "int";
                else if (next.tokenType == Global_Vars.realTokenType) toReturn.type.type = "real";
                else if (next.tokenType == Global_Vars.longrealTokenType) toReturn.type.type = "longreal";
            }
            else if (next.tokenType == 24)//21.3
            {
                toReturn.productionNum = "21.3";
                tokenQueue.Dequeue();
                Wrapper factor = parseFactor();
                errorCheck(toReturn, factor);
            }
            else
            {
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
            toReturn.follows.Add("else");
            toReturn.follows.Add(";");
            toReturn.follows.Add("end");
            toReturn.follows.Add("relop");
            Token next = tokenQueue.Peek();
            if (next.tokenType == 9)
            {
                if (next.attribute == "(")// 21.4
                {
                    toReturn.productionNum = "21.4";
                    tokenQueue.Dequeue();
                    Wrapper expressionList = parseExpressionList();
                    errorCheck(toReturn, expressionList);
                    next = tokenQueue.Peek();
                    if (next.tokenType == 9 && next.attribute == ")")
                    {
                        tokenQueue.Dequeue();
                        //Correct state, do nothing
                    }
                    else
                    {
                        reportParserError(toReturn.expected, next.lexeme, next.lineNum);
                        toReturn.type.type = "ERR*";
                        errorRecov(toReturn);
                    }

                }
                else if (next.attribute == "[")//21.5
                {
                    toReturn.productionNum = "21.5";
                    tokenQueue.Dequeue();
                    Wrapper expression = parseExpression();
                    errorCheck(toReturn, expression);
                    next = tokenQueue.Peek();
                    if (next.tokenType == 9 && next.attribute == "]")
                    {
                        tokenQueue.Dequeue();
                        //Correct state, do nothing
                    }
                    else
                    {
                        reportParserError(toReturn.expected, next.lexeme, next.lineNum);
                        toReturn.type.type = "ERR*";
                        errorRecov(toReturn);
                    }
                }
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
                reportParserError(toReturn.expected, next.lexeme, next.lineNum);
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
            Token next = tokenQueue.Peek();
            Boolean foundFollows = false;
            while (!foundFollows)
            {
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
            return true;
        }

        private bool addBlueNode(string id, string type)
        {
            return true;
        }

        private blueNode getBlueNode(string currentId)
        {
            throw new NotImplementedException();
        }
    }
}
