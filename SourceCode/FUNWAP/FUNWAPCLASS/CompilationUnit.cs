
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;


namespace FUNWAPCLASS
{  /// <summary>
 
    /// </summary>
    public abstract class CompilationUnit
    {
       
        public abstract SYMBOL_INFO Execute(RUNTIME_CONTEXT cont, ArrayList actuals);

        public abstract bool Compile(DNET_EXECUTABLE_GENERATION_CONTEXT cont);
    }

    /// <summary>
    ///    Abstract base class for Procedure
    ///    All the statements in a Program ( Compilation unit )
    ///    will be compiled into a PROC 
    /// </summary>
    public abstract class PROC
    {
        //
      
        public abstract SYMBOL_INFO Execute(RUNTIME_CONTEXT cont, ArrayList formals);

        public abstract bool Compile(DNET_EXECUTABLE_GENERATION_CONTEXT cont);

    }

    /// <summary>
    ///     A CodeModule is a Compilation Unit ..
    ///     At this point of time ..it is just a bunch
    ///     of statements... 
    /// </summary>
    public class TModule : CompilationUnit
    {
        /// <summary>
        ///    A Program is a collection of Procedures...
        ///    Now , we support only global function...
        /// </summary>
        private ArrayList m_procs = null;
        /// <summary>
        ///    List of Compiled Procedures....
        ///    At this point of time..only one procedure
        ///    will be there....
        /// </summary>
        private ArrayList compiled_procs = null;
        /// <summary>
        ///    class to generate IL executable... 
        /// </summary>

        private ExeGenerator _exe = null;
        public static string PrewFun = "";
        public static int ClosureSymbol;
        /// <summary>
        ///    Ctor for the Program ...
        /// </summary>
        /// <param name="procedures"></param>

        public TModule(ArrayList procs)
        {
            m_procs = procs;

        }

        
        /// <summary>
        ///      
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public bool CreateExecutable(string name)
        {
            //
            // Create an instance of Exe Generator
            // ExeGenerator takes a TModule and 
            // exe name as the Parameter...
            _exe = new ExeGenerator(this, name);
            // Compile The module...
            Compile(null);
            // Save the Executable...
            _exe.Save();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cont"></param>
        /// <returns></returns>
        /// 

        public override bool Compile(DNET_EXECUTABLE_GENERATION_CONTEXT cont)
        {
            compiled_procs = new ArrayList();
            foreach (Procedure p in m_procs)
            {
                DNET_EXECUTABLE_GENERATION_CONTEXT con = new DNET_EXECUTABLE_GENERATION_CONTEXT(this, p, _exe.type_bulder);
                compiled_procs.Add(con);
                p.Compile(con);

            }
            return true;

        }
       
        public override SYMBOL_INFO Execute(RUNTIME_CONTEXT cont, ArrayList actuals)
        {
            Procedure p = Find("Main");

            if (p != null)
            {

                return p.Execute(cont, actuals);
            }

            return null;

        }

        public MethodBuilder _get_entry_point(string _funcname)
        {
            foreach (DNET_EXECUTABLE_GENERATION_CONTEXT u in compiled_procs)
            {
                if (u.MethodName.Equals(_funcname))
                {
                    return u.MethodHandle;
                }

            }

            return null;


        }

        public Procedure Find(string str)
        {
            string pname;

            foreach (Procedure p in m_procs)
            {
                if (p.clossure == null)
                    pname = p.Name;
                else
                    pname = p.clossure.Name;
             
                if (pname.ToUpper().CompareTo(str.ToUpper()) == 0)
                    return p;

            }

            return null;

        }


    }

    ///
    ///
    public class Procedure : PROC,ICloneable
    {
        /// <summary>
        ///    Procedure name ..which defaults to Main 
        ///    in the type MainClass
        /// </summary>
        public string m_name;
        /// <summary>
        ///    Formal parameters...
        /// </summary>
        public ArrayList m_formals = null;
        /// <summary>
        ///     List of statements which comprises the Procedure
        /// </summary>
        public ArrayList m_statements = null;
        /// <summary>
        ///     Local variables
        /// </summary>
        public SymbolTable m_locals = null;
        /// <summary>
        ///      
        /// </summary>
        public SYMBOL_INFO return_value = null;
        /// <summary>
        ///       TYPE_INFO => TYPE_NUMERIC
        /// </summary>
        public TYPE_INFO _type = TYPE_INFO.TYPE_ILLEGAL;
       
        public Procedure clossure = null;
        public static int ClosureSymbolInt = 0;

        public static RUNTIME_CONTEXT ClosureMentain = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="formals"></param>
        /// <param name="stats"></param>
        /// <param name="locals"></param>
        /// <param name="type"></param>

        public Procedure(string name,
                         ArrayList formals,
                         ArrayList stats,
                         SymbolTable locals,
                         TYPE_INFO type)
        {
            m_name = name;
            
         
            m_formals = formals;
            m_statements = stats;
            m_locals = locals;
            _type = type;
        }

        public Procedure(string name,
                      ArrayList formals,
                      ArrayList stats,
                      SymbolTable locals,
                     TYPE_INFO type,
            Procedure clsr)
        {
            m_name = name;
            //
           
            m_formals = formals;
            m_statements = stats;
            m_locals = locals;
            _type = type;
            clossure = clsr;
        }
        /// <summary>
        /// to copy Proedure Object
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        /// <summary>
        /// 
        /// </summary>
        public TYPE_INFO TYPE
        {

            get
            {
                return _type;

            }

        }
        
        public ArrayList FORMALS
        {
            get
            {
                return m_formals;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        // public string Name
        public string Name
        {
            set
            {
                m_name = value;
                //Name = value;
            }

            get
            {
                return m_name;
            }

        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SYMBOL_INFO ReturnValue()
        {
            return return_value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cont"></param>
        /// <returns></returns>
        public TYPE_INFO TypeCheck(COMPILATION_CONTEXT cont)
        {

            return TYPE_INFO.TYPE_NUMERIC;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cont"></param>
        /// <returns></returns>
        public override bool Compile(DNET_EXECUTABLE_GENERATION_CONTEXT cont)
        {

            if (m_formals != null)
            {


                int i = 0;

                foreach (SYMBOL_INFO b in m_formals)
                {

                    System.Type type = (b.Type == TYPE_INFO.TYPE_BOOL) ?
                        typeof(bool) : (b.Type == TYPE_INFO.TYPE_NUMERIC) ?
                        typeof(double) : typeof(string);
                    int s = cont.DeclareLocal(type);
                    b.loc_position = s;
                    cont.TABLE.Add(b);
                    cont.CodeOutput.Emit(OpCodes.Ldarg, i);
                    cont.CodeOutput.Emit(OpCodes.Stloc, cont.GetLocal(s));
                    i++;
                }

            }


            foreach (Stmt e1 in m_statements)
            {
                e1.Compile(cont);
            }

            cont.CodeOutput.Emit(OpCodes.Ret);
            return true;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cont"></param>
        /// <param name="actuals"></param>
        /// <returns></returns>
        public override SYMBOL_INFO Execute(RUNTIME_CONTEXT cont, ArrayList actuals)
        {
            ArrayList vars = new ArrayList();
            int i = 0;

          
           
            if (m_formals != null && actuals != null)
            {

                i = 0;
                foreach (SYMBOL_INFO b in m_formals)
                {

                    SYMBOL_INFO inf = actuals[i] as SYMBOL_INFO;
                    inf.SymbolName = b.SymbolName;
                    cont.TABLE.Add(inf);
                    i++;

                }

            }
            
            foreach (Stmt e1 in m_statements)
            {
                return_value = e1.Execute(cont);

                if (return_value != null)
                    return return_value;

            }

        

            return null;

        }
    }
}

