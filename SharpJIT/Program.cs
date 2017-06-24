using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Core;
using SharpJIT.Core.Objects;

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
                var voidType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Void);

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
                //var instructions = new List<Instruction>
                //{
                //    new Instruction(OpCodes.LoadInt, 10),
                //    new Instruction(OpCodes.NewArray, intType.Name),
                //    new Instruction(OpCodes.Call, "std.println", new List<BaseType>() { intArrayType }),
                //    new Instruction(OpCodes.LoadInt, 0),
                //    new Instruction(OpCodes.Return)
                //};

                var pointMetadata = new ClassMetadata("Point");
                pointMetadata.DefineField(new FieldDefinition("x", intType, AccessModifier.Public));
                pointMetadata.DefineField(new FieldDefinition("y", intType, AccessModifier.Public));
                pointMetadata.CreateFields();

                container.VirtualMachine.ClassMetadataProvider.Add(pointMetadata);

                var pointType = container.VirtualMachine.TypeProvider.FindClassType("Point");

                var constructorFunction = new ManagedFunction(
                    new FunctionDefinition(".constructor", new List<BaseType>(), voidType, pointType, true),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.Return)
                    });

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                    new Instruction(OpCodes.LoadField, "Point::x"),
                    new Instruction(OpCodes.Return)
                };

                var assembly = new Assembly(
                    "program",
                    new ManagedFunction(def, new List<BaseType>() { }, instructions),
                    constructorFunction);

                container.VirtualMachine.LoadAssemblyInternal(assembly);
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
