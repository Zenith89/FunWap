using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FUNWAPCLASS;


namespace FUNWAP_INTERPERET
{
    class Program
    {

        static void TestFileScript(string filename)
        {

            if (filename == null)
                return;

            string programs = null; ;
            // -------------- Read the contents from the file

            StreamReader sr = new StreamReader(filename);
            programs = sr.ReadToEnd();

            sr.Close();
            sr.Dispose();


            //---------------- Creates the Parser Object
            // With Program text as argument

            RDParser pars = null;
            pars = new RDParser(programs);
            TModule p = null;
            p = pars.DoParse();

            if (p == null)
            {
                Console.WriteLine("Parse Process Failed");
                return;
            }
            //
            //  Now that Parse is Successul...
            //  Do a recursive interpretation...!
            //
            RUNTIME_CONTEXT f = new RUNTIME_CONTEXT(p);
            SYMBOL_INFO fp = p.Execute(f, null);

        }


        static void Main(string[] args)
        {
            if (args == null ||
                args.Length != 1)
            {
                Console.WriteLine("FUNWAP INTERPERATER <scriptname>\n");
                return;

            }
            TestFileScript(args[0]);
            //------------- Wait for the Key Press
            Console.Read();
        }
    }
}
