using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicStructGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            const int noFieldsToSpawn = 16383;      // currently (Aug 2019) max number of fields that can be included when 
                                              // the type is generated is 64K, however if one uses that type to pass
                                              // it as parameter, it will get an "JIT Compiler encountered an internal
                                              // limitation" unless the number of fields within is at most 16383
            // Code used from https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.typebuilder?view=netframework-4.8
            AssemblyName aName = new AssemblyName("DynamicStructAssembly");
            AssemblyBuilder ab =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    aName,
                    AssemblyBuilderAccess.RunAndSave);

            // For a single-module assembly, the module name is usually
            // the assembly name plus an extension.
            ModuleBuilder mb =
                ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");

            TypeBuilder tb = mb.DefineType(
                "MyDynamicStruct",
                 TypeAttributes.Public,
                 typeof(ValueType));
            for (int i = 0; i < noFieldsToSpawn; i++)
            {
                // Add a private field of type int (Int32).
                FieldBuilder fbNumber = tb.DefineField(
                    "m_number" + i.ToString(),
                    typeof(int),
                    FieldAttributes.Public);
            }
            
            // Finish the type.
            Type t = tb.CreateType();

            FieldInfo[] fields = t.GetFields();

            // Save the single-module assembly
            ab.Save(aName.Name + ".dll");

            // === Test for filling all the fields with a value ===
            // Create an instance from the defined type
            object o1 = Activator.CreateInstance(t);
            // Write to each field
            for (int i = 0; i < noFieldsToSpawn; i++)
            {
                fields[i].SetValue(o1, 5);
            }
            

        }
    }
}
