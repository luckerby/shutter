using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ConsumeDynamicStruct
{
    class Program
    {
        static public MyDynamicStruct bigStruct = new MyDynamicStruct();

        static void Main(string[] args)
        {
            // FieldInfo's SetValue operates on an object, which means
            //  that simply passing it a struct will box it, and change
            //  the fields on that boxed copy, then discarding the output.
            //  We need to be smart (just like here https://social.msdn.microsoft.com/Forums/vstudio/en-US/33284e33-d004-4b76-bc0f-50100ec46bf1/fieldinfosetvalue-dont-work-in-struct?forum=csharpgeneral)
            //   and box ourselves, keep a reference to the boxed struct,
            //   modify the fields using that reference, then assign
            //   the unboxed struct to our original struct

            // Box our struct
            object boxedBigStruct = bigStruct;

            // Initialize the variable
            FieldInfo[] allFields = typeof(MyDynamicStruct).GetFields();
            for(int i=0; i<allFields.Length;i++)
            {
                allFields[i].SetValue(boxedBigStruct, 1);
            }
            bigStruct = (MyDynamicStruct)boxedBigStruct;

            Console.WriteLine("Initialization complete");

            Thread.Sleep(500);
            RunAgainstCopiedData(bigStruct);

            // Wait so that dotTrace can initiate a snapshot
            Console.WriteLine("done");
            Console.ReadLine();
        }

        static void RunAgainstCopiedData(MyDynamicStruct param)
        {
            Thread.Sleep(500);
            // Same workaround is performed here
            object boxedParamStruct = param;

            // Set all the fields of the passed Struct to a different value
            FieldInfo[] allFields = typeof(MyDynamicStruct).GetFields();
            for (int i = 0; i < allFields.Length; i++)
            {
                allFields[i].SetValue(boxedParamStruct, 2);
            }

            param = (MyDynamicStruct)boxedParamStruct;

        }
    }
}
