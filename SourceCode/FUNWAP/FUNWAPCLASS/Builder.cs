﻿
using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace FUNWAPCLASS
{
    /// <summary>
    ///    Base class for all the Builder
    ///    
    /// 
    /// 
    ///            AbstractBuilder
    ///                     TModuleBuilder
    ///                     ProcedureBuilder
    /// </summary>
    public class AbstractBuilder
    {

    }
    /// <summary>
    ///      A Builder for Creating a Module
    /// </summary>
    class TModuleBuilder : AbstractBuilder
    {
        /// <summary>
        ///     Array of Procs 
        /// </summary>
        private ArrayList procs;
        /// <summary>
        ///    Array of Function Prototypes
        ///    not much use as of now...
        /// </summary>
        private ArrayList protos = null;

        /// <summary>
        ///     Ctor does not do much
        /// </summary>
        public TModuleBuilder()
        {
            procs = new ArrayList();
            protos = new ArrayList();
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsFunction(string name)
        {
            foreach (FUNCTION_INFO fpinfo in protos)
            {
                if (fpinfo._name.Equals(name))
                {
                    return true;
                }

            }

            return false;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ret_type"></param>
        /// <param name="type_infos"></param>
        /// 
       
        public void AddFunctionProtoType(string name, TYPE_INFO ret_type,
            ArrayList type_infos)
        {
            FUNCTION_INFO info = new FUNCTION_INFO(name, ret_type, type_infos);
            protos.Add(info);
        }


       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ret_type"></param>
        /// <param name="type_infos"></param>
        /// <returns></returns>
        /// 
      
        public bool CheckFunctionProtoType(string name, TYPE_INFO ret_type,
            ArrayList type_infos)
        {
            foreach (FUNCTION_INFO fpinfo in protos)
            {
                if (fpinfo._name.Equals(name))
                {
                    if (fpinfo._ret_value == ret_type)
                    {
                        if (type_infos.Count == fpinfo._typeinfo.Count)
                        {
                            int i = 0;
                            for (i = 0; i < type_infos.Count; ++i)
                            {
                                TYPE_INFO a = (TYPE_INFO)type_infos[i];
                                TYPE_INFO b = (TYPE_INFO)type_infos[i];

                                if (a != b)
                                    return false;

                            }

                            return true;

                        }


                    }

                }

            }

            return false;

        }
        /// <summary>
        ///     Add Procedure
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Add(Procedure p)
        {
            procs.Add(p);
            return true;
        }

        /// <summary>
        ///      Create Program 
        /// </summary>
        /// <returns></returns>
        public TModule GetProgram()
        {
            return new TModule(procs);
        }

        ///
        ///
        ///
        public Procedure GetProc(string name)
        {
            foreach (Procedure p in procs)
            {
                if (p.Name.Equals(name))
                {
                    return p;
                }

            }

            return null;

        }
       

    }



    /// <summary>
    ///    
    /// </summary>
    public class ProcedureBuilder : AbstractBuilder
    {
        /// <summary>
        ///    Procedure name ..now it is hard coded
        ///    to MAIN
        /// </summary>
        private string proc_name = "";
        /// <summary>
        ///    Compilation context for type analysis
        /// </summary>
        COMPILATION_CONTEXT ctx = null;
        /// <summary>
        ///     Procedure arguments
        ///   
        /// </summary>
        ArrayList m_formals = new ArrayList();
        /// <summary>
        ///    Array of Statements
        /// </summary>
        ArrayList m_stmts = new ArrayList();
        /// <summary>
        ///    Return Type of the procedure
        /// </summary>
        TYPE_INFO inf = TYPE_INFO.TYPE_ILLEGAL;

        ProcedureBuilder closure = null;
        bool isClosure = false;





        public ProcedureBuilder(string name, COMPILATION_CONTEXT _ctx)
        {

            ctx = _ctx;
            proc_name = name;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool AddLocal(SYMBOL_INFO info)
        {
            ctx.TABLE.Add(info);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>

        public bool AddFormals(SYMBOL_INFO info)
        {
            m_formals.Add(info);
            return true;
        }
        public ArrayList GetFormals()
        {
            return m_formals;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public TYPE_INFO TypeCheck(Exp e)
        {
            return e.TypeCheck(ctx);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="st"></param>
        public void AddStatement(Stmt st)
        {
            m_stmts.Add(st);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strname"></param>
        /// <returns></returns>
        public SYMBOL_INFO GetSymbol(string strname)
        {

            return ctx.TABLE.Get(strname);

        }

        /// <summary>
        ///   Check the function Prototype
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckProto(string name)
        {
            return true;

        }
        /// <summary>
        /// Return value set/get
        /// </summary>
        /// // public ReTURN-TYPE TYPE 
        public TYPE_INFO TYPE
        {
            get
            {
                return inf;
            }

            set
            {
                inf = value;

            }


        }
     

        public ProcedureBuilder clsoure
        {
            get
            {
                return closure;
            }
            set { closure = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// 
        public void InitClsr()
        {
            isClosure = true;
            closure = new ProcedureBuilder("", ctx);
        }
        public SymbolTable TABLE
        {
            get
            {
                return ctx.TABLE;
            }
        }

        public COMPILATION_CONTEXT Context
        {
            get
            {
                return ctx;
            }
            ///use for closure in the ret stmt to acces the father function...
            set
            {
                ctx=value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get
            {
                return proc_name;
            }

            set
            {
                proc_name = value;

            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Procedure GetProcedure()
        {
            Procedure ret = null;
            if (isClosure)
                goto closureInit;

            ret = new Procedure(proc_name, m_formals, m_stmts, ctx.TABLE, inf); ;
            return ret;

        closureInit:
            Procedure closure = new Procedure(this.clsoure.Name, this.clsoure.m_formals, this.clsoure.m_stmts, this.clsoure.Context.TABLE, this.clsoure.inf);
            ret = new Procedure(proc_name, m_formals, m_stmts, ctx.TABLE, inf, closure);

            return ret;
        }





    }


}
