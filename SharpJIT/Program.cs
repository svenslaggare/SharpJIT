using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Core;

namespace SharpJIT
{
	class Program
	{
        static void Main(string[] args)
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
                var intArrayType = container.VirtualMachine.TypeProvider.FindArrayType(intType);
                var def = new FunctionDefinition("main", new List<BaseType>(), intType);

                //var instructions = new List<Instruction>
                //{
                //    new Instruction(OpCodes.LoadFloat, 2.5f),
                //    new Instruction(OpCodes.LoadFloat, 1.35f),
                //    new Instruction(OpCodes.AddFloat),
                //    new Instruction(OpCodes.Call, "std.println", Enumerable.Repeat(floatType, 1).ToList()),
                //    new Instruction(OpCodes.LoadInt, 0),
                //    new Instruction(OpCodes.Return)
                //};
                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 10),
                    new Instruction(OpCodes.NewArray, intType.Name),
                    //new Instruction(OpCodes.Pop),
                    new Instruction(OpCodes.Call, "std.println", new List<BaseType>() { intArrayType }),
                    new Instruction(OpCodes.LoadInt, 0),
                    new Instruction(OpCodes.Return)
                };

                var assembly = Assembly.SingleFunction(new ManagedFunction(def, new List<BaseType>() { }, instructions));

                container.LoadAssembly(assembly);
                container.VirtualMachine.Compile();

                foreach (var function in assembly.Functions)
                {
                    var disassembler = new Disassembler(
                        container.VirtualMachine.Compiler.GetCompilationData(function),
                        x => new Compiler.Win64.Disassembler(x));
                    Console.WriteLine(disassembler.Disassemble());
                }

                int returnValue = container.VirtualMachine.GetEntryPoint()();
                Console.WriteLine(returnValue);
            }

            Console.ReadLine();
        }
    }
}
