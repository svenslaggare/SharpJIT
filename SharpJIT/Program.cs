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
        static void Main2(string[] args)
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
                var voidType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Void);

                var intArrayType = container.VirtualMachine.TypeProvider.FindArrayType(intType);

                var pointMetadata = new ClassMetadata("Point");
                pointMetadata.DefineField(new FieldDefinition("x", intType, AccessModifier.Public));
                pointMetadata.DefineField(new FieldDefinition("y", intType, AccessModifier.Public));
                pointMetadata.CreateFields();
                container.VirtualMachine.ClassMetadataProvider.Add(pointMetadata);
                var pointType = container.VirtualMachine.TypeProvider.FindClassType("Point");

                //var constructorFunction = new ManagedFunction(
                //    new FunctionDefinition(".constructor", new List<BaseType>(), voidType, pointType, true),
                //    new List<BaseType>(),
                //    new List<Instruction>()
                //    {
                //        new Instruction(OpCodes.Return)
                //    });

                //var mainFunction = new ManagedFunction(
                //    new FunctionDefinition("main", new List<BaseType>(), intType),
                //    new List<BaseType>() { },
                //    new List<Instruction>
                //    {
                //        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                //        new Instruction(OpCodes.LoadField, "Point::x"),
                //        new Instruction(OpCodes.Return)
                //    });

                //var functions = new List<ManagedFunction>()
                //{
                //    mainFunction,
                //    constructorFunction
                //};

                var testFunction2 = new ManagedFunction(
                    new FunctionDefinition("test2", new List<BaseType>() { intType }, intType),
                    new List<BaseType>() { intType, floatType, pointType, intArrayType },
                    new List<Instruction>
                    {
                        new Instruction(OpCodes.LoadFloat, 13.37f),
                        new Instruction(OpCodes.StoreLocal, 1),
                        new Instruction(OpCodes.LoadInt, 10),
                        new Instruction(OpCodes.NewArray, intType.Name),
                        new Instruction(OpCodes.StoreLocal, 3),
                        new Instruction(OpCodes.LoadArgument, 0),
                        new Instruction(OpCodes.LoadInt, 2),
                        new Instruction(OpCodes.AddInt),
                        new Instruction(OpCodes.Return)
                    });

                var testFunction = new ManagedFunction(
                    new FunctionDefinition("test", new List<BaseType>(), intType),
                    new List<BaseType>() { },
                    new List<Instruction>
                    {
                        new Instruction(OpCodes.LoadInt, 4711),
                        new Instruction(OpCodes.Call, "test2", new List<BaseType>() { intType }),
                        new Instruction(OpCodes.Return)
                    });

                var mainFunction = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { },
                    new List<Instruction>
                    {
                        new Instruction(OpCodes.LoadInt, 4711),
                        new Instruction(OpCodes.Pop),
                        new Instruction(OpCodes.Call, "test", new List<BaseType>()),
                        new Instruction(OpCodes.Return)
                    });

                var functions = new List<ManagedFunction>()
                {
                    mainFunction,
                    testFunction,
                    testFunction2
                };

                container.VirtualMachine.LoadFunctionsAsAssembly(functions);
                container.VirtualMachine.Compile();

                foreach (var function in functions)
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

        public delegate void PrintlnPoint(long objectReference);

        static void Main(string[] args)
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var voidType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Void);

                var pointMetadata = new ClassMetadata("Point");
                pointMetadata.DefineField(new FieldDefinition("x", intType, AccessModifier.Public));
                pointMetadata.DefineField(new FieldDefinition("y", intType, AccessModifier.Public));
                pointMetadata.CreateFields();
                container.VirtualMachine.ClassMetadataProvider.Add(pointMetadata);
                var pointType = container.VirtualMachine.TypeProvider.FindClassType("Point");

                var pointArrayType = container.VirtualMachine.TypeProvider.FindArrayType(pointType);

                void Println(long objectReference)
                {
                    Console.WriteLine($"0x{objectReference.ToString("x8")}");
                }

                container.VirtualMachine.Binder.Define(FunctionDefinition.NewExternal<PrintlnPoint>(
                    "std.println",
                    new List<BaseType>() { pointType },
                    voidType,
                    Println));

                var constructorFunction = new ManagedFunction(
                    new FunctionDefinition(".constructor", new List<BaseType>(), voidType, pointType, true),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        //new Instruction(OpCodes.LoadArgument, 0),
                        //new Instruction(OpCodes.LoadInt, 1337),
                        //new Instruction(OpCodes.StoreField, "Point::x"),
                        new Instruction(OpCodes.Return)
                    });

                var mainFunction = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { pointArrayType },
                    //new List<BaseType>() { pointType },
                    new List<Instruction>
                    {
                        //new Instruction(OpCodes.LoadInt, 10),
                        //new Instruction(OpCodes.NewArray, pointType.Name),
                        //new Instruction(OpCodes.StoreLocal, 0),
                        //new Instruction(OpCodes.LoadLocal, 0),
                        //new Instruction(OpCodes.LoadInt, 0),
                        //new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        //new Instruction(OpCodes.StoreElement, pointType.Name),
                        //new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
                        //new Instruction(OpCodes.LoadInt, 0),
                        //new Instruction(OpCodes.Return),

                        new Instruction(OpCodes.LoadInt, 10),
                        new Instruction(OpCodes.NewArray, pointType.Name),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.Pop),
                        new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
                        new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return),
                    });

                var functions = new List<ManagedFunction>()
                {
                    mainFunction,
                    constructorFunction
                };

                container.VirtualMachine.LoadFunctionsAsAssembly(functions);
                container.VirtualMachine.Compile();

                foreach (var function in functions)
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
