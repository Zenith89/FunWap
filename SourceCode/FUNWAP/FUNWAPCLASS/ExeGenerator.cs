
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace FUNWAPCLASS
{  /// <summary>
    ///    ExeGenerator - Takes care of the creation of 
    ///    .NET executable...
    ///    
    /// </summary>
    public class ExeGenerator
    {
        /// <summary>
        ///  Hierarchy is as follows..
        ///    Assembly 
        ///        Module
        ///           Type 
        ///              Method 
        ///   Refer to Reflection.Emit documentation for 
        ///   more details on creation of .NET executable
        /// </summary>
        AssemblyBuilder _asm_builder = null;
        ModuleBuilder _module_builder = null;
        TypeBuilder _type_builder = null;
       
        /// <summary>
        ///     Name of the Executable 
        /// </summary>
        string _name = "";

        /// <summary>
        ///     Program to be Compiled...
        /// </summary>
        TModule _p = null;
        

       
        /// <summary>
        ///     Ctor which takes Executable name 
        ///     as parameter
        /// </summary>
        /// <param name="name"></param>
        public ExeGenerator(TModule  p, string name)
        {
            //
            // The Program to be compiled...
            //
            _p = p;
            //
            // Get The App Domain
            //
            //
            AppDomain _app_domain = Thread.GetDomain();
            AssemblyName _asm_name = new AssemblyName();
            //
            //  One can give a strong name , if we want
            //
            _asm_name.Name = "MyAssembly";
            //
            // Save the Exe Name
            //
            _name = name;
            //
            // Create an instance of Assembly Builder
            //
            //
            _asm_builder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                _asm_name,
                AssemblyBuilderAccess.RunAndSave);
            //
            // Create a module builder , from AssemblyBuilder
            //
            _module_builder = _asm_builder.DefineDynamicModule("DynamicModule1", _name, false);
            //
            // Create a class by the name MainClass..
            // We compile the statements into a static method
            // of the type MainClass .. the entry point will
            // be called Main
            // ExeGenerator will be called from TModule.Compile method
            // We will add methods to the type MainClass as static method
            // 
            _type_builder = _module_builder.DefineType("MainClass");

        }
        /// <summary>
        ///   return the type builder....
        /// </summary>
        public TypeBuilder type_bulder
        {

            get
            {
                return _type_builder;
            }
        }

       


        /// <summary>
        /// 
        /// </summary>

        public void Save()
        {
            //
            //  Note :- Call this (Save ) method only after 
            //  Compilation of All statements....
            _type_builder.CreateType();

            //
            // Retrieve the Entry Point from TModule....
            //
            MethodBuilder mb = _p._get_entry_point("MAIN");

           if (mb != null)
            {
               //
               // Here we will set the Assembly as a Console Application...
               // We will also set the Entry Point....
               //
                _asm_builder.SetEntryPoint(mb, PEFileKinds.ConsoleApplication);
            }
            //
            // Write the Resulting Executable...
            //
            
            _asm_builder.Save(_name);
        }
   }
}
