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
        /// <summary>
        /// Creates an add function with that takes the given amount of arguments
        /// </summary>
        private static Function CreateAddFunction(Win64Container container, int numArgs, bool optimize = false)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var def = new FunctionDefinition("add", Enumerable.Repeat(intType, numArgs).ToList(), intType);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadArgument, 0)
            };
            for (int i = 1; i < numArgs; i++)
            {
                instructions.Add(new Instruction(OpCodes.LoadArgument, i));
                instructions.Add(new Instruction(OpCodes.AddInt));
            }

            instructions.Add(new Instruction(OpCodes.Return));

            return new Function(def, instructions, new List<BaseType>());
        }

        /// <summary>
        /// Creates a loop call add function
        /// </summary>
        private static Function CreateLoopCallAdd(Win64Container container, int count)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var def = new FunctionDefinition("main", new List<BaseType>(), intType);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadInt, count),
                new Instruction(OpCodes.StoreLocal, 0),

                new Instruction(OpCodes.LoadInt, 1),
                new Instruction(OpCodes.LoadLocal, 1),
                new Instruction(OpCodes.Call, "add", Enumerable.Repeat(intType, 2).ToList()),
                new Instruction(OpCodes.StoreLocal, 1),

                new Instruction(OpCodes.LoadLocal, 0),
                new Instruction(OpCodes.LoadInt, 1),
                new Instruction(OpCodes.SubInt),
                new Instruction(OpCodes.StoreLocal, 0),
                new Instruction(OpCodes.LoadLocal, 0),

                new Instruction(OpCodes.LoadInt, 0),
                new Instruction(OpCodes.BranchGreaterThan, 2),

                new Instruction(OpCodes.LoadLocal, 1),
                new Instruction(OpCodes.Return)
            };
            return new Function(def, instructions, new List<BaseType>() { intType, intType });
        }

        /// <summary>
        /// Creates the fibonacci function
        /// </summary>
        private static Function CreateFibFunction(Win64Container container)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var def = new FunctionDefinition("fib", Enumerable.Repeat(intType, 1).ToList(), intType);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadArgument, 0),
                new Instruction(OpCodes.LoadInt, 1),
                new Instruction(OpCodes.BranchGreaterThan, 5),
                new Instruction(OpCodes.LoadArgument, 0),
                new Instruction(OpCodes.Return),

                new Instruction(OpCodes.LoadArgument, 0),
                new Instruction(OpCodes.LoadInt, 2),
                new Instruction(OpCodes.SubInt),
                new Instruction(OpCodes.Call, "fib", Enumerable.Repeat(intType, 1).ToList()),

                new Instruction(OpCodes.LoadArgument, 0),
                new Instruction(OpCodes.LoadInt, 1),
                new Instruction(OpCodes.SubInt),
                new Instruction(OpCodes.Call, "fib", Enumerable.Repeat(intType, 1).ToList()),

                new Instruction(OpCodes.AddInt),
                new Instruction(OpCodes.Return)
            };
            return new Function(def, instructions, new List<BaseType>());
        }

        /// <summary>
        /// Creates the fibonacci function
        /// </summary>
        private static Function CreateSumFunction(Win64Container container)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var def = new FunctionDefinition("sum", Enumerable.Repeat(intType, 1).ToList(), intType);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadArgument, 0),
                new Instruction(OpCodes.LoadInt, 0),
                new Instruction(OpCodes.BranchNotEqual, 5),
                new Instruction(OpCodes.LoadInt, 0),
                new Instruction(OpCodes.Return),

                new Instruction(OpCodes.LoadArgument, 0),
                new Instruction(OpCodes.LoadInt, 1),
                new Instruction(OpCodes.SubInt),
                new Instruction(OpCodes.Call, "sum", Enumerable.Repeat(intType, 1).ToList()),

                new Instruction(OpCodes.LoadArgument, 0),
                new Instruction(OpCodes.AddInt),
                new Instruction(OpCodes.Return)
            };
            return new Function(def, instructions, new List<BaseType>());
        }

        /// <summary>
        /// Creates the main function
        /// </summary>
        private static Function CreateMainFunction(Win64Container container, string toCall, int n)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var def = new FunctionDefinition("main", new List<BaseType>(), intType);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadInt, n),
                new Instruction(OpCodes.Call, toCall, Enumerable.Repeat(intType, 1).ToList()),
                new Instruction(OpCodes.Return)
            };
            return new Function(def, instructions, new List<BaseType>());
        }

        static void Main(string[] args)
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
                var def = new FunctionDefinition("main", new List<BaseType>(), intType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.5f),
                    new Instruction(OpCodes.LoadFloat, 1.35f),
                    new Instruction(OpCodes.AddFloat),
                    new Instruction(OpCodes.Call, "std.println", Enumerable.Repeat(floatType, 1).ToList()),
                    new Instruction(OpCodes.LoadInt, 0),
                    new Instruction(OpCodes.Return)
                };
                var assembly = Assembly.SingleFunction(new Function(def, instructions, new List<BaseType>()));

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

        //static void Main(string[] args)
        //{
        //    using (var container = new Win64Container())
        //    {
        //        //container.VirtualMachine.Settings["NumIntRegisters"] = 0;
        //        //bool optimize = true;
        //        //var assembly = new Assembly(
        //        //    CreateAddFunction(container, 2, optimize),
        //        //    CreateLoopCallAdd(container, 30000000, optimize));

        //        //var assembly = new Assembly(
        //        //    CreateFibFunction(container, optimize),
        //        //    CreateMainFunction(container, "fib", 40, optimize));

        //        var intType = container.VirtualMachine.TypeProvider.GetPrimitiveType(PrimitiveTypes.Int);
        //        var funcDef = new FunctionDefinition("main", new List<VMType>(), intType);

        //        var instructions = new List<Instruction>();

        //        instructions.Add(new Instruction(OpCodes.LoadInt, 3));
        //        instructions.Add(new Instruction(OpCodes.LoadInt, 8));
        //        instructions.Add(new Instruction(OpCodes.LoadInt, 2));
        //        instructions.Add(new Instruction(OpCodes.DivInt));
        //        instructions.Add(new Instruction(OpCodes.MulInt));
        //        instructions.Add(new Instruction(OpCodes.Ret));

        //        var func = new Function(funcDef, instructions, new List<VMType>());
        //        func.Optimize = true;
        //        var assembly = Assembly.SingleFunction(func);

        //        container.LoadAssembly(assembly);
        //        container.VirtualMachine.Compile();

        //        foreach (var function in assembly.Functions)
        //        {
        //            var disassembler = new Disassembler(
        //                container.VirtualMachine.Compiler.GetCompilationData(function),
        //                x => new Compiler.Win64.Disassembler(x));
        //            Console.WriteLine(disassembler.Disassemble());
        //        }

        //        var stopwatch = new Stopwatch();
        //        stopwatch.Start();
        //        int returnValue = container.VirtualMachine.GetEntryPoint()();
        //        var elapsed = stopwatch.Elapsed;

        //        Console.WriteLine(returnValue);
        //        Console.WriteLine(elapsed.TotalMilliseconds);
        //    }

        //    Console.ReadLine();
        //}
    }
}
