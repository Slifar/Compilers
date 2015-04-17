using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilers_Project.RDParser
{
    class Parser
    {
        Queue<Token> tokenQueue = Global_Vars.tokenQueue;
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
                            Wrapper program2 = parseProgram2();
                            toReturn.children.Add(program2);
                            if (program2.type.type == "ERR" || idList.type.type == "ERR*")
                            {
                                toReturn.type.type = "ERR";
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
                        tokenQueue.Dequeue();
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
                    tokenQueue.Dequeue();
                    next = tokenQueue.Peek();
                    if (next.tokenType == 9 && next.attribute == ":")
                    {
                        tokenQueue.Dequeue();
                        Wrapper type = parseType();
                        errorCheck(toReturn, type);
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
            throw new NotImplementedException();
        }
        private Wrapper parseCompoundStatement()
        {
            throw new NotImplementedException();
        }

        private Wrapper parseSubprogramDecarations()
        {
            throw new NotImplementedException();
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
                toReturn.productionNum = "15.1";
                tokenQueue.Dequeue();
                Wrapper variable2 = parseVariable2();
                errorCheck(toReturn, variable2);
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
                    tokenQueue.Dequeue();
                    Wrapper procedureStatement2 = parseProcedureStatement2();
                    errorCheck(toReturn, procedureStatement2);
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
                Wrapper expression = parseExpression();
                errorCheck(toReturn, expression);
                Wrapper expressionList2 = parseExpressionList2();
                errorCheck(toReturn, expressionList2);
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
                tokenQueue.Dequeue();
                Wrapper expression = parseExpression();
                errorCheck(toReturn, expression);
                Wrapper expressionList2 = parseExpressionList2();
                errorCheck(toReturn, expressionList2);
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
                Wrapper expression2 = parseExpression2();
                errorCheck(toReturn, expression2);
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
                toReturn.productionNum = "20.1"
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
                    tokenQueue.Dequeue();
                    Wrapper factor = parseFactor();
                    errorCheck(toReturn, factor);
                    Wrapper term2 = parseTerm2();
                    errorCheck(toReturn, term2);
                }
                else //Numerical operation, requires numbers.
                {
                    tokenQueue.Dequeue();
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
        }
    }
}
