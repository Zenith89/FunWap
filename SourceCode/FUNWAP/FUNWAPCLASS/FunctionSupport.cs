using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace FUNWAPCLASS
{
    /// <summary>
    ///      FUNCTION info
    /// </summary>
  public class FUNCTION_INFO
    {
        public TYPE_INFO _ret_value;
        public string _name;
        public ArrayList _typeinfo;
       
        FUNCTION_INFO closure;

        public FUNCTION_INFO(string name, TYPE_INFO ret_value,
            ArrayList formals)
        {

            _ret_value = ret_value;
            _typeinfo = formals;
            _name = name;
        }
        //fun outsideAdder()fun(int)int
        public FUNCTION_INFO(string name, FUNCTION_INFO ret_value,
            ArrayList formals)
        {

            closure = ret_value;
            _typeinfo = formals;
            _name = name;
            
        }
    }

    

}
