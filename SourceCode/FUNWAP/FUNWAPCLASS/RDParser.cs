
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace FUNWAPCLASS
{
  
    public class RDParser : Lexer
    {

        /// <summary>
        /// it is  RECURSIVE DECENT PARSER; The Final outcome of the parser is a group of 
        ///    functions.
        /// </summary>
        TModuleBuilder prog = null;
      

        public RDParser(String str)
            : base(str)
        {
            prog = new TModuleBuilder();
        }

    /// <summary>
    /// <BExpr> ::= <LExpr> <LOGIC_OP> <LExpr>
    /// </summary>
    /// <param name="pb"></param>
    /// <returns></returns>
        public Exp BExpr(ProcedureBuilder pb)
        {
            TOKEN l_token;
            Exp RetValue = LExpr(pb);
            while (Current_Token == TOKEN.TOK_AND || Current_Token == TOKEN.TOK_OR)
            {
                l_token = Current_Token;
                Current_Token = GetNext();
                Exp e2 = LExpr(pb);
                RetValue = new LogicalExp(l_token, RetValue, e2);

            }
            return RetValue;

        }
        /// <summary>
        /// <LExpr> ::= <Expr> <REL_OP ><Expr>
        /// </summary>
        /// <returns></returns>

        public Exp LExpr(ProcedureBuilder pb)
        {
            TOKEN l_token;
            Exp RetValue = Expr(pb);
            while (Current_Token == TOKEN.TOK_GT ||
                    Current_Token == TOKEN.TOK_LT ||
                    Current_Token == TOKEN.TOK_GTE ||
                    Current_Token == TOKEN.TOK_LTE ||
                    Current_Token == TOKEN.TOK_NEQ ||
                    Current_Token == TOKEN.TOK_EQ)
            {
                l_token = Current_Token;
                Current_Token = GetNext();
                Exp e2 = Expr(pb);
                RELATION_OPERATOR relop = GetRelOp(l_token);
                RetValue = new RelationExp(relop, RetValue, e2);


            }
            return RetValue;

        }

        /// <summary>
        ///    <Expr>  ::=  <Term> | <Term> <ADD_OP> <Expr>
        ///    
        /// </summary>
        /// <returns></returns>
        public Exp Expr(ProcedureBuilder ctx)
        {
            TOKEN l_token;
            Exp RetValue = Term(ctx);
            while (Current_Token == TOKEN.TOK_PLUS || Current_Token == TOKEN.TOK_SUB)
            {
                l_token = Current_Token;
                Current_Token = GetToken();
                Exp e1 = Expr(ctx);

                if (l_token == TOKEN.TOK_PLUS)
                    RetValue = new BinaryPlus(RetValue, e1);
                else
                    RetValue = new BinaryMinus(RetValue, e1);
            }

            return RetValue;

        }
        /// <summary>
        ///<Term>::= <Factor> <MUL_OP> <Term>
        /// </summary>
        public Exp Term(ProcedureBuilder ctx)
        {
            TOKEN l_token;
            Exp RetValue = Factor(ctx);

            while (Current_Token == TOKEN.TOK_MUL || Current_Token == TOKEN.TOK_DIV)
            {
                l_token = Current_Token;
                Current_Token = GetToken();


                Exp e1 = Term(ctx);
                if (l_token == TOKEN.TOK_MUL)
                    RetValue = new Mul(RetValue, e1);
                else
                    RetValue = new Div(RetValue, e1);

            }

            return RetValue;
        }

        /// <summary>
        /// <Factor> ::= <numeric> | <string> | true | false | identifier | <callexpr> | “(“ <expr> “)” | {+|-|!}
        /// </summary>
        public Exp Factor(ProcedureBuilder ctx)
        {
            TOKEN l_token;
            Exp RetValue = null;



            if (Current_Token == TOKEN.TOK_NUMERIC)
            {

                RetValue = new NumericConstant(GetNumber());
                Current_Token = GetToken();

            }
            else if (Current_Token == TOKEN.TOK_STRING)
            {
                RetValue = new StringLiteral(last_str);
                Current_Token = GetToken();
            }
            else if (Current_Token == TOKEN.TOK_BOOL_FALSE ||
                      Current_Token == TOKEN.TOK_BOOL_TRUE)
            {
                RetValue = new BooleanConstant(
                    Current_Token == TOKEN.TOK_BOOL_TRUE ? true : false);
                Current_Token = GetToken();
            }
            else if (Current_Token == TOKEN.TOK_OPAREN)
            {

                Current_Token = GetToken();

                RetValue = BExpr(ctx);  // Recurse

                if (Current_Token != TOKEN.TOK_CPAREN)
                {
                    Console.WriteLine("Missing Closing Parenthesis\n");
                    throw new Exception();

                }
                Current_Token = GetToken();
            }

            else if (Current_Token == TOKEN.TOK_PLUS || Current_Token == TOKEN.TOK_SUB)
            {
                l_token = Current_Token;
                Current_Token = GetToken();
                RetValue = Factor(ctx);
                if (l_token == TOKEN.TOK_PLUS)
                    RetValue = new UnaryPlus(RetValue);
                else
                    RetValue = new UnaryMinus(RetValue);

            }
            else if (Current_Token == TOKEN.TOK_NOT)
            {
                l_token = Current_Token;
                Current_Token = GetToken();
                RetValue = Factor(ctx);

                RetValue = new LogicalNot(RetValue);
            }
            else if (Current_Token == TOKEN.TOK_UNQUOTED_STRING)
            {
                String str = base.last_str;


                // if it is not a function..it ought to 
                // be a variable

                if (!prog.IsFunction(str))
                {
                    //
                    // if it is not a function..it ought to 
                    // be a variable...
                    SYMBOL_INFO inf = ctx.GetSymbol(str);
                                        
                    if (inf == null)
                        throw new Exception("Undefined symbol");

                    GetNext();
                    return new Variable(inf);
                }

                //
                // P can be null , if we are parsing a
                // recursive function call
                //
                Procedure p = prog.GetProc(str);
                // It is a Function Call
                // Parse the function invocation
                //
                Exp ptr = ParseCallProc(ctx, p);
                GetNext();
                return ptr;
            }





            else
            {

                Console.WriteLine("Illegal Token");
                throw new Exception();
            }


            return RetValue;

        }
        /// <summary>
        /// <callexpr> ::= identifier “(“ <callpar> “)” “;”
        /// 
        /// <returns></returns>
        ///
        public Exp ParseCallProc(ProcedureBuilder pb, Procedure p)
        {
            GetNext();

            if (Current_Token != TOKEN.TOK_OPAREN)
            {
                throw new Exception("Opening Parenthesis expected");
            }

            GetNext();

            ArrayList actualparams = new ArrayList();

            while (true)
            {
                // Evaluate Each Expression in the 
                // parameter list and populate actualparams
                // list
                Exp exp = BExpr(pb);
                // do type analysis
                exp.TypeCheck(pb.Context);
                // if , there are more parameters
                if (Current_Token == TOKEN.TOK_COMMA)
                {
                    actualparams.Add(exp);
                    GetNext();
                    continue;
                }


                if (Current_Token != TOKEN.TOK_CPAREN)
                {
                    throw new Exception("Expected paranthesis");
                }

                else
                {
                    // Add the last parameters
                    actualparams.Add(exp);
                    break;

                }
            }
            
            if (p != null)
                return new CallExp(p, actualparams);
            else
                return new CallExp(pb.Name,
                                   true,  // recurse !
                                   actualparams);

        }




        /// <summary>
        ///   The new Parser entry point
        /// </summary>
        /// <returns></returns>
        public TModule DoParse()
        {
            try
            {
                GetNext();   // Get The First Valid Token
                return ParseFunctions();
            }
            catch (Exception e)
            {
                Console.WriteLine("Parse Error -------");
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        /// <summary>
        ///  <Module>::= {<Procudure>}+
        ///  While There are more functions to parse
        /// </summary>
        /// <returns></returns>
        public TModule ParseFunctions()
        {
           
            while (Current_Token == TOKEN.TOK_FUNCTION)
            {
                ProcedureBuilder b = ParseFunction();
                Procedure s = b.GetProcedure();

                if (s == null)
                {
                    Console.WriteLine("Error While Parsing Functions");
                    return null;
                }

                prog.Add(s);
                GetNext();
            }

            //
            //  Convert the builder into a program
            //
            return prog.GetProgram();
        }

        /// <summary>
        /// <Procudure>::= fun identifier “(“ <arglist>“)” <retType> <Statments>
        ///    Parse A Single Function.
        /// </summary>
        /// <returns></returns> 
        //List of argument type 
        ProcedureBuilder ParseFunction()
        {
            ProcedureBuilder p = new ProcedureBuilder("", new COMPILATION_CONTEXT());
          
            if (Current_Token != TOKEN.TOK_FUNCTION)
                return null;

         

            GetNext();
            // parse function name
            if (Current_Token != TOKEN.TOK_UNQUOTED_STRING)
            {
                return null;
            }
            p.Name = this.last_str;
            GetNext();//name

            ///////////////////now  parse for parameters////////////////////////////

            if (Current_Token != TOKEN.TOK_OPAREN)
            {
                return null;
            }
            GetNext(); // remove (
            //(a numeric)||()
            ArrayList lst_types = new ArrayList();
           
            while (Current_Token == TOKEN.TOK_UNQUOTED_STRING)
            {
                SYMBOL_INFO inf = new SYMBOL_INFO();
                inf.SymbolName = last_str;

                GetNext();

                if (Current_Token == TOKEN.TOK_VAR_BOOL ||
                     Current_Token == TOKEN.TOK_VAR_NUMBER ||
                     Current_Token == TOKEN.TOK_VAR_STRING)
                {
                    inf.Type = (Current_Token == TOKEN.TOK_VAR_BOOL) ?
                        TYPE_INFO.TYPE_BOOL : (Current_Token == TOKEN.TOK_VAR_NUMBER) ?
                        TYPE_INFO.TYPE_NUMERIC : TYPE_INFO.TYPE_STRING;
                }
                else
                    return null;

                               
                lst_types.Add(inf.Type);
                p.AddFormals(inf);
                p.AddLocal(inf);
            
                GetNext();
                if (Current_Token != TOKEN.TOK_COMMA)
                {
                    break;
                   
                }
                GetNext();
            }

            if (Current_Token != TOKEN.TOK_CPAREN)
            {//(a int ...
                return null;
            }
            //(a int) int


            GetNext();


            ///parse for return type (= number/bool/function)

            if (!(Current_Token == TOKEN.TOK_VAR_BOOL ||
                Current_Token == TOKEN.TOK_VAR_NUMBER ||
                Current_Token == TOKEN.TOK_VAR_STRING || 
                Current_Token == TOKEN.TOK_FUNCTION))
            {
                return null;

            }

            ///-------- Assign the return type
            
               p.TYPE = (Current_Token == TOKEN.TOK_VAR_BOOL) ?
                TYPE_INFO.TYPE_BOOL : (Current_Token == TOKEN.TOK_VAR_NUMBER) ?
                TYPE_INFO.TYPE_NUMERIC : (Current_Token == TOKEN.TOK_FUNCTION) ?TYPE_INFO.TYPE_FUNCTION:TYPE_INFO.TYPE_STRING;

            //FUNCTION_INFO cls= new FUNCTION_INFO(""
            prog.AddFunctionProtoType(p.Name, p.TYPE, lst_types);
            
            GetNext(); // remove return type

              
        /////////////////////addStmts///////
       
            //if (Current_Token != TOKEN.TOK_OCBR)
            //    return null;

            if (p.TYPE == TYPE_INFO.TYPE_FUNCTION)
            {
                while (Current_Token != TOKEN.TOK_OCBR)
                    GetNext();
            }
            if (Current_Token != TOKEN.TOK_OCBR)
                return null;
            GetNext();// remove {


            ArrayList lst = StatementList(p);
            
            
            if (Current_Token != TOKEN.TOK_CCBR)
            {
                throw new Exception("} expected in the function");
            }

            // Accumulate all statements to 
            // Procedure builder
            //
            
            foreach (Stmt s in lst )
            {
                p.AddStatement(s);

            }

            return p;
        }
        

        /// <summary>
        /// <Statments>::= {<stmt>}+
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
     
          
        private ArrayList StatementList(ProcedureBuilder ctx)
        {
            ArrayList arr = new ArrayList();
          
            while ((Current_Token != TOKEN.TOK_CCBR) && (Current_Token != TOKEN.TOK_ELSE))
            {
                Stmt temp = Statement(ctx);
                if (temp != null)
                {
                    arr.Add(temp);
                }
            }
            return arr;
        }

        /// <summary>
        /// 
        /// <stmt>::= <varDeclarestmt>|<Printstmt>|<assigmentstmt> | <callstmt> | <ifstmt> | <whilestmt > | <retstmt>
        ///    This Routine Queries Statement Type 
        ///    to take the appropriate Branch...
        ///    Currently , only Print and PrintLine statement
        ///    are supported..
        ///    if a line does not start with Print or PrintLine ..
        ///    an exception is thrown
        /// </summary>
        /// <returns></returns>
        private Stmt Statement(ProcedureBuilder ctx)
        {
            Stmt retval = null;
            switch (Current_Token)
            {
                
                case TOKEN.TOK_VAR:
                    retval = ParseVariableDeclStatement(ctx);
                    GetNext();
                    return retval;
                case TOKEN.TOK_PRINT:
                    retval = ParsePrintStatement(ctx);
                    GetNext();
                    break;
                case TOKEN.TOK_PRINTLN:
                    retval = ParsePrintLNStatement(ctx);
                    GetNext();
                    break;
                case TOKEN.TOK_UNQUOTED_STRING:
                    retval = ParseAssignmentStatement(ctx);
                    GetNext();
                    return retval;
                case TOKEN.TOK_IF:
                    retval = ParseIfStatement(ctx);
                    GetNext();
                    return retval;

                case TOKEN.TOK_WHILE:
                    retval = ParseWhileStatement(ctx);
                    GetNext();
                    return retval;
                case TOKEN.TOK_RETURN:
                    retval = ParseReturnStatement(ctx);
                    GetNext();
                    return retval;
                case TOKEN.TOK_EOF: break;

                default:
                    throw new Exception("in function"+ctx.Name+" \tInvalid statement");

            }
            return retval;
        }
        /// <summary>
        ///    Parse the Print Staement .. The grammar is 
        ///    <Printstmt>::=print <expr>“;”
      
        private Stmt ParsePrintStatement(ProcedureBuilder ctx)
        {
            GetNext();
            Exp a = BExpr(ctx);
            //
            // Do the type analysis ...
            //
            a.TypeCheck(ctx.Context);

            if (Current_Token != TOKEN.TOK_SEMI)
            {
                throw new Exception("in function"+ctx.Name+"\t; is expected");
            }
            return new PrintStatement(a);
        }
        /// <summary>
        ///    Parse the PrintLine Staement .. The grammar is 
        ///   <Printstmt>::=printline <Bexpr>“;”
        ///    
        /// </summary>
        /// <returns></returns>
        private Stmt ParsePrintLNStatement(ProcedureBuilder ctx)
        {
            GetNext();
            Exp a = Expr(ctx);
            a.TypeCheck(ctx.Context);
            if (Current_Token != TOKEN.TOK_SEMI)
            {
                throw new Exception("in function  "+ctx.Name+" \t; is expected" );
            }
            return new PrintLineStatement(a);
        }

        /// <summary>
        ///    Parse Variable declaration statement
        ///    <varDeclarestmt>::=  var  identifier  <type>  “ ; “ | var  identifier <type>=<expr> “;”
        ///                          | var  identifier fun =<callexpr>  “;”

        ///    var a numeric / var a numeric=8;
        /// </summary>
        /// <param name="type"></param>

        public Stmt ParseVariableDeclStatement(ProcedureBuilder ctx)
        {


            GetNext();//remove var


            if (Current_Token == TOKEN.TOK_UNQUOTED_STRING)
            {
                GetNext();
                if (Current_Token == TOKEN.TOK_VAR_BOOL || Current_Token == TOKEN.TOK_VAR_STRING || Current_Token == TOKEN.TOK_VAR_NUMBER || Current_Token == TOKEN.TOK_FUNCTION)
                {
                    SYMBOL_INFO symb = new SYMBOL_INFO();
                    symb.SymbolName = base.last_str;// last str = variable name (a)
                     string   newfnme= base.last_str;
                    symb.Type = (Current_Token == TOKEN.TOK_VAR_BOOL) ?
                                TYPE_INFO.TYPE_BOOL : (Current_Token == TOKEN.TOK_VAR_NUMBER) ?
                                TYPE_INFO.TYPE_NUMERIC : (Current_Token == TOKEN.TOK_FUNCTION) ? TYPE_INFO.TYPE_FUNCTION : TYPE_INFO.TYPE_STRING;
                    GetNext();
                    // now we are at ; or =
                    ////////////////////////////////////////var a numeric = 8;////////////////////////////////////////
                    if (Current_Token == TOKEN.TOK_ASSIGN)
                    {
                        ctx.TABLE.Add(symb);

                        if (symb.Type == TYPE_INFO.TYPE_FUNCTION)
                        {
                            GetNext();//remove =
                            GetNext();
                            string fname = base.last_str;
                            bool check = prog.IsFunction(fname);
                            if (check)
                            {
                                Procedure varfun = null;
                                varfun = (Procedure)prog.GetProc(fname).Clone();
                        
                                 varfun.Name = newfnme;
                                 prog.AddFunctionProtoType(newfnme, varfun.TYPE, varfun.m_formals);
                                prog.Add(varfun);

                                 while (Current_Token != TOKEN.TOK_SEMI) GetNext();
                                        return null;
                            }
                        }
                        ctx.TABLE.Add(symb);

                        new VariableDeclStatement(symb);
                        GetNext();// remove =
                        bool IsAsync = false;

                        if (Current_Token == TOKEN.TOK_ASYNC)
                        {
                            IsAsync = true;
                            while (Current_Token != TOKEN.TOK_UNQUOTED_STRING)
                                GetNext();
                        }
                        Exp exp = BExpr(ctx);

                        //------------ Do the type analysis ...


                        if (exp.TypeCheck(ctx.Context) != symb.Type)
                        {
                            throw new Exception("in function  " + ctx.Name + "\t Type mismatch in assignment");

                        }

                        // -------------- End of statement ( ; ) is expected
                        if (Current_Token != TOKEN.TOK_SEMI)
                        {
                           
                            throw new Exception("in function" + ctx.Name +   "\t; expected");
                        }

                        if (IsAsync)
                        {
                            GetNext();//GetNext();//remove return n }
                        }

                        return new AssignmentStatement(symb, exp, IsAsync);

                    }
                    /////// // var a numeric;
                    else if (Current_Token == TOKEN.TOK_SEMI)
                    {
                        // ----------- Add to the Symbol Table
                        ctx.TABLE.Add(symb);

                        //  return the Object of type
                        //  VariableDeclStatement
                        // This will just store the Variable name
                        // in the symbol table
                        return new VariableDeclStatement(symb);
                    }
                    else
                    {
                        throw new Exception("in function" + ctx.Name + "\t ; expected");
                       
                    }
                }
                else
                {
                    throw new Exception("in function" + ctx.Name + " \t Invalid DataType \n Numeric,Bool or String is Expected");
                    
                }
            }

            else
            {
                throw new Exception("in function" + ctx.Name + "\t invalid variable declaration or ; expected");

            }
        }
        /// <summary>
        ///    Parse the Assignment Statement 
        ///   <assigmentstmt>::= identifier “=“ <Bexpr> “;” |  identifier “=“ <Async> 
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public Stmt ParseAssignmentStatement(ProcedureBuilder ctx)
        {

            //
            // Retrieve the variable and look it up in 
            // the symbol table ..if not found throw exception
            //


            string variable = base.last_str;
            SYMBOL_INFO s = ctx.TABLE.Get(variable);
            if (s == null)
            {
                throw new Exception("in function  " + ctx.Name + "\t Variable not found ");
               
            }

            //------------ The next token ought to be an assignment
            // expression....

            GetNext();

            if (Current_Token != TOKEN.TOK_ASSIGN)
            {
                throw new Exception("in function " + ctx.Name + "\t = expected ");
               

            }

            //-------- Skip the token to start the expression
            // parsing on the RHS
            GetNext();// remove =
            
           bool IsAsync=false ;
            ///***************/////////
            if (Current_Token == TOKEN.TOK_ASYNC)
            {
                IsAsync = true;
                while (Current_Token != TOKEN.TOK_UNQUOTED_STRING)
                    GetNext();
            }
            //****************////////////////********

            Exp exp = BExpr(ctx);

            //------------ Do the type analysis ...


            if (exp.TypeCheck(ctx.Context) != s.Type)
            {
                throw new Exception("in function  " + ctx.Name + "\tType mismatch in assignment");

            }

            // -------------- End of statement ( ; ) is expected
            if (Current_Token != TOKEN.TOK_SEMI)
            {
                throw new Exception("in function  " + ctx.Name + "\t; expected");
                

            }

            if (IsAsync)
            { GetNext();//GetNext();//remove return n }
            }
           
            return new AssignmentStatement(s, exp,IsAsync);

        }
      
        /// <summary>
        /// <ifstmt>::=if  <Bexpr>  “{“<Statments> } [ else “{“<Statments>“}”] 
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public Stmt ParseIfStatement(ProcedureBuilder pb)
        {
            GetNext();
            ArrayList true_part = null;
            ArrayList false_part = null;
            Exp exp = BExpr(pb);  // Evaluate Expression


            if (pb.TypeCheck(exp) != TYPE_INFO.TYPE_BOOL)
            {
                throw new Exception("in function  " + pb.Name + "\tExpects a boolean expression");

            }

              if (Current_Token != TOKEN.TOK_OCBR)
            {
                throw new Exception("in function "+pb.Name+ "\t { Expected in if stmt");
               
            }
            // skip {
            GetNext();

            true_part = StatementList(pb);//{.......}
            GetNext(); // skip }
            

            // no else stmt  
            if (Current_Token == TOKEN.TOK_CCBR)
            {// return fals stmt as null
                return new IfStatement(exp, true_part, false_part);
            }


            if (Current_Token != TOKEN.TOK_ELSE)
            {

                throw new Exception("in function  " + pb.Name + "\tELSE expected");
            }
            GetNext();// remove else
            GetNext();// remove {
            false_part = StatementList(pb);

            if (Current_Token != TOKEN.TOK_CCBR)
            {
                throw new Exception("in function" + pb.Name + "\t} Expected");

            }

            return new IfStatement(exp, true_part, false_part);

        }
     
        /// <summary>
        /// <whilestmt>::= while “(“  <Bexpr>  ”)”  “{“ <Statments>  “}”
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public Stmt ParseWhileStatement(ProcedureBuilder pb)
        {

            GetNext();

            Exp exp = BExpr(pb);
            if (pb.TypeCheck(exp) != TYPE_INFO.TYPE_BOOL)
            {
                throw new Exception("Expects a boolean expression");

            }
        
            if (Current_Token != TOKEN.TOK_OCBR)
            {
                throw new Exception("{ Expected in if stmt");
               


            }
            GetNext();//Skip {
            /////******///
            ArrayList body = StatementList(pb);

           
            if ((Current_Token != TOKEN.TOK_CCBR))
            {
                throw new Exception("} Expected ");
                

            }


            return new WhileStatement(exp, body);

        }
        
        /// <summary>
        /// <rexpr>=<Bexpr> |fun “(“ <arglist>“)” <retType> <Statments> “;”
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public Stmt ParseReturnStatement(ProcedureBuilder pb)
        {
            
            GetNext();
            //return fun (x numeric)numeric{.....
            if(Current_Token==TOKEN.TOK_FUNCTION)
            {
                GetNext();
                pb.InitClsr();
                pb.clsoure.Name = pb.Name + "1";
               
                ///////////////////now  parse for parameters////////////////////////////

                if (Current_Token != TOKEN.TOK_OPAREN)
                {
                    return null;
                }
                GetNext(); // remove (
                //(a int)||()
                ArrayList lst_types = new ArrayList();
           
                while (Current_Token == TOKEN.TOK_UNQUOTED_STRING)
                {
                    SYMBOL_INFO inf = new SYMBOL_INFO();
                    inf.SymbolName = last_str;

                    GetNext();

                    if (Current_Token == TOKEN.TOK_VAR_BOOL ||
                         Current_Token == TOKEN.TOK_VAR_NUMBER ||
                         Current_Token == TOKEN.TOK_VAR_STRING)
                    {
                        inf.Type = (Current_Token == TOKEN.TOK_VAR_BOOL) ?
                            TYPE_INFO.TYPE_BOOL : (Current_Token == TOKEN.TOK_VAR_NUMBER) ?
                            TYPE_INFO.TYPE_NUMERIC : TYPE_INFO.TYPE_STRING;
                    }
                    else
                        return null;
   
                    lst_types.Add(inf.Type);
                    pb.clsoure.AddFormals(inf);
                    pb.clsoure.AddLocal(inf);
            
                    GetNext();
                    if (Current_Token != TOKEN.TOK_COMMA)
                    {
                        break;
                   
                    }
                    GetNext();
                }

                if (Current_Token != TOKEN.TOK_CPAREN)
                {
                    return null;
                }
           
                   GetNext();

                //parse for return type (= number/bool/function)

                if (!(Current_Token == TOKEN.TOK_VAR_BOOL ||
                    Current_Token == TOKEN.TOK_VAR_NUMBER ||
                    Current_Token == TOKEN.TOK_VAR_STRING || 
                    Current_Token == TOKEN.TOK_FUNCTION))
                {
                    return null;

                }

                ///-------- Assign the return type
            
                   pb.clsoure.TYPE = (Current_Token == TOKEN.TOK_VAR_BOOL) ?
                    TYPE_INFO.TYPE_BOOL : (Current_Token == TOKEN.TOK_VAR_NUMBER) ?
                    TYPE_INFO.TYPE_NUMERIC : (Current_Token == TOKEN.TOK_FUNCTION) ?TYPE_INFO.TYPE_FUNCTION:TYPE_INFO.TYPE_STRING;
                    ////////////////////////////////////

                         GetNext();
                         if (Current_Token != TOKEN.TOK_OCBR)
                         { return null; }
                GetNext();
                    ////***********/********************************************
                ///add statments
                    ArrayList lst = StatementList(pb.clsoure);
                  

                    foreach (Stmt st in lst)
                    {
                        pb.clsoure.AddStatement(st);
                       

                    }
                    //ClsrStmt:
                    if (Current_Token != TOKEN.TOK_CCBR)
                    {
                        throw new Exception("} expected in the function");
                    }

                return null;

              
            }



            else
            {



                Exp exp = BExpr(pb);
                if (Current_Token != TOKEN.TOK_SEMI)
                {
                    throw new Exception(pb.Name +"; expected ");
                    

                }
                pb.TypeCheck(exp);
              
                return new ReturnStatement(exp);

            }
        }
        /// <summary>
        ///    Convert a token to Relational Operator
        /// </summary>
        /// <param name="tok"></param>
        /// <returns></returns>
        private RELATION_OPERATOR GetRelOp(TOKEN tok)
        {
            if (tok == TOKEN.TOK_EQ)
                return RELATION_OPERATOR.TOK_EQ;
            else if (tok == TOKEN.TOK_NEQ)
                return RELATION_OPERATOR.TOK_NEQ;
            else if (tok == TOKEN.TOK_GT)
                return RELATION_OPERATOR.TOK_GT;
            else if (tok == TOKEN.TOK_GTE)
                return RELATION_OPERATOR.TOK_GTE;
            else if (tok == TOKEN.TOK_LT)
                return RELATION_OPERATOR.TOK_LT;
            else
                return RELATION_OPERATOR.TOK_LTE;


        }


    }
}

