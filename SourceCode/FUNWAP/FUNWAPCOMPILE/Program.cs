using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using FUNWAPCLASS;
using System.IO;
namespace FUNWAPCOMPILE
{
    class Program
    {

        static void TestFileScript(string filename)
        {

            if (filename == null)
                return;


            // -------------- Read the contents from the file

            StreamReader sr = new StreamReader(filename);
            string programs = sr.ReadToEnd();
            sr.Close();
            sr.Dispose(); 

            //---------------- Creates the Parser Object
            // With Program text as argument 


           
                        RDParser pars = null;
            
            pars = new RDParser(programs);
            //Fib.SL
            TModule p = null;
            p = pars.DoParse();


            if (p == null)
            {
                Console.WriteLine("Parse Process Failed ");
                return;
            }



            //
            //  Now that Parse is Successul...
            //  Create an Executable...!
         

            if (p.CreateExecutable("outPut.exe"))
            {
                Console.WriteLine("Creation of Executable is successul");
                // Save the Assembly and generate the MSIL code with ILDASM.EXE
                string modName =  "outPut.exe";
                Process process = new Process();
                process.StartInfo.FileName = "ildasm.exe";
                process.StartInfo.Arguments = "/text /nobar \"" + modName ;
                process.StartInfo.UseShellExecute = false;// set false if u want to use  RedirectStandardOutput Property
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                string s = process.StandardOutput.ReadToEnd();
                Console.WriteLine(s);
               
                string[] fname = modName.Split('.');
           
                System.IO.StreamWriter file = new System.IO.StreamWriter(fname[0] +".txt");
                file.WriteLine(s);

                file.Close();

                //////
                //Console.ReadLine();
                process.WaitForExit();
                process.Close();
                return;
            }
        }
        static void Main(string[] args)
        {
            if (args == null ||
                args.Length != 1)
            {
                Console.WriteLine("FUNWAP COMPILE <scriptname>\n");
                return;

            }
            TestFileScript(args[0]);
            //------------- Wait for the Key Press
            Console.Read();
        }
    }
}
