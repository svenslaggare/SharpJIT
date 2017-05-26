using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpJIT;
using SharpJIT.Core;

namespace SharpJIT.Test
{
    /// <summary>
    /// Generates test programs
    /// </summary>
    public static class TestProgramGenerator
    {
        /// <summary>
        /// A simple function without any control flow
        /// </summary>
        public static Function Simple(Win64Container container)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadInt, 2),
                new Instruction(OpCodes.LoadInt, 4),
                new Instruction(OpCodes.AddInt),
                new Instruction(OpCodes.Return)
            };
            return new Function(
                new FunctionDefinition("main", new List<BaseType>() { }, intType),
                instructions,
                new List<BaseType>());
        }

        /// <summary>
        /// A simple function without any control flow
        /// </summary>
        public static Function Simple2(Win64Container container)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadInt, 2),
                new Instruction(OpCodes.LoadInt, 4),
                new Instruction(OpCodes.LoadInt, 6),
                new Instruction(OpCodes.AddInt),
                new Instruction(OpCodes.AddInt),
                new Instruction(OpCodes.Return)
            };
            return new Function(
                new FunctionDefinition("main", new List<BaseType>() { }, intType),
                instructions,
                new List<BaseType>());
        }

        /// <summary>
        /// A simple function without any control flow
        /// </summary>
        public static Function Simple3(Win64Container container)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadInt, 1),
                new Instruction(OpCodes.LoadInt, 2),
                new Instruction(OpCodes.AddInt),
                new Instruction(OpCodes.LoadInt, 3),
                new Instruction(OpCodes.AddInt),
                new Instruction(OpCodes.LoadInt, 4),
                new Instruction(OpCodes.AddInt),
                new Instruction(OpCodes.LoadInt, 5),
                new Instruction(OpCodes.AddInt),
                new Instruction(OpCodes.Return)
            };
            return new Function(
                new FunctionDefinition("main", new List<BaseType>() { }, intType),
                instructions,
                new List<BaseType>());
        }

        /// <summary>
        /// A function with branches
        /// </summary>
        public static Function Branch(Win64Container container)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadInt, 4),
                new Instruction(OpCodes.LoadInt, 2),
                new Instruction(OpCodes.BranchEqual, 6),

                new Instruction(OpCodes.LoadInt, 5),
                new Instruction(OpCodes.StoreLocal, 0),
                new Instruction(OpCodes.Branch, 8),

                new Instruction(OpCodes.LoadInt, 15),
                new Instruction(OpCodes.StoreLocal, 0),

                new Instruction(OpCodes.LoadLocal, 0),
                new Instruction(OpCodes.Return)
            };
            return new Function(
                new FunctionDefinition("main", new List<BaseType>() { }, intType),
                instructions,
                new List<BaseType>() { intType });
        }

        /// <summary>
        /// A function with multiple returns
        /// </summary>
        public static Function MultipleReturns(Win64Container container)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadInt, 4),
                new Instruction(OpCodes.Return),

                new Instruction(OpCodes.LoadInt, 5),
                new Instruction(OpCodes.Return)
            };
            return new Function(
                new FunctionDefinition("main", new List<BaseType>() { }, intType),
                instructions,
                new List<BaseType>() { intType });
        }

        /// <summary>
        /// The max function
        /// </summary>
        public static Function Max(Win64Container container)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadArgument, 0),
                new Instruction(OpCodes.LoadArgument, 1),
                new Instruction(OpCodes.BranchGreaterThan, 6),

                new Instruction(OpCodes.LoadArgument, 1),
                new Instruction(OpCodes.StoreLocal, 0),
                new Instruction(OpCodes.Branch, 9),

                new Instruction(OpCodes.LoadArgument, 0),
                new Instruction(OpCodes.StoreLocal, 0),
                new Instruction(OpCodes.Branch, 9),

                new Instruction(OpCodes.LoadLocal, 0),
                new Instruction(OpCodes.Return)
            };
            return new Function(
                new FunctionDefinition("max", new List<BaseType>() { intType, intType }, intType),
                instructions,
                new List<BaseType>() { intType });
        }

        /// <summary>
        /// Function with locals with none overlapping life time
        /// </summary>
        public static Function Locals(Win64Container container)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadInt, 2),
                new Instruction(OpCodes.StoreLocal, 0),

                new Instruction(OpCodes.LoadInt, 4),
                new Instruction(OpCodes.StoreLocal, 1),

                new Instruction(OpCodes.LoadLocal, 1),
                new Instruction(OpCodes.Return)
            };
            return new Function(
                new FunctionDefinition("main", new List<BaseType>(), intType),
                instructions,
                new List<BaseType>() { intType, intType });
        }

        /// <summary>
        /// Function with float locals with none overlapping life time
        /// </summary>
        public static Function FloatLocals(Win64Container container)
        {
            var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadFloat, 2.0f),
                new Instruction(OpCodes.StoreLocal, 0),

                new Instruction(OpCodes.LoadFloat, 4.0f),
                new Instruction(OpCodes.StoreLocal, 1),

                new Instruction(OpCodes.LoadLocal, 1),
                new Instruction(OpCodes.Return)
            };
            return new Function(
                new FunctionDefinition("floatMain", new List<BaseType>(), floatType),
                instructions,
                new List<BaseType>() { floatType, floatType });
        }

        /// <summary>
        /// Creates a function that counts up to the given amount
        /// </summary>
        public static Function LoopCount(Win64Container container, int count)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var def = new FunctionDefinition("main", new List<BaseType>() { }, intType);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadInt, count),
                new Instruction(OpCodes.StoreLocal, 0),

                new Instruction(OpCodes.LoadInt, 1),
                new Instruction(OpCodes.LoadLocal, 1),
                new Instruction(OpCodes.AddInt),
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
        /// Creates a sum function without a loop
        /// </summary>
        public static Function SumNoneLoop(Win64Container container, int count)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var def = new FunctionDefinition("main", new List<BaseType>(), intType);

            var instructions = new List<Instruction>();

            for (int i = 1; i <= count; i++)
            {
                instructions.Add(new Instruction(OpCodes.LoadInt, i));
            }

            for (int i = 0; i < count - 1; i++)
            {
                instructions.Add(new Instruction(OpCodes.AddInt));
            }

            instructions.Add(new Instruction(OpCodes.Return));

            return new Function(def, instructions, new List<BaseType>());
        }


        /// <summary>
        /// Creates a negative sum function without a loop
        /// </summary>
        public static Function NegativeSumNoneLoop(Win64Container container, int count)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var def = new FunctionDefinition("main", new List<BaseType>(), intType);

            var instructions = new List<Instruction>();

            for (int i = 1; i <= count; i++)
            {
                instructions.Add(new Instruction(OpCodes.LoadInt, i));
            }

            for (int i = 0; i < count - 1; i++)
            {
                instructions.Add(new Instruction(OpCodes.SubInt));
            }

            instructions.Add(new Instruction(OpCodes.Return));

            return new Function(def, instructions, new List<BaseType>());
        }

        /// <summary>
        /// Computes the result for the negative sum function
        /// </summary>
        public static int NegativeSumResult(int count)
        {
            var stack = new Stack<int>();

            for (int i = 1; i <= count; i++)
            {
                stack.Push(i);
            }

            for (int i = 0; i < count - 1; i++)
            {
                var op2 = stack.Pop();
                var op1 = stack.Pop();
                stack.Push(op1 - op2);
            }

            return stack.Pop();
        }

        /// <summary>
        /// Creates a product function without a loop
        /// </summary>
        public static Function ProductNoneLoop(Win64Container container, int count)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var def = new FunctionDefinition("main", new List<BaseType>(), intType);

            var instructions = new List<Instruction>();

            for (int i = 1; i <= count; i++)
            {
                instructions.Add(new Instruction(OpCodes.LoadInt, i));
            }

            for (int i = 0; i < count - 1; i++)
            {
                instructions.Add(new Instruction(OpCodes.MulInt));
            }

            instructions.Add(new Instruction(OpCodes.Return));

            return new Function(def, instructions, new List<BaseType>());
        }

        /// <summary>
        /// Creates a sum function without a loop using locals
        /// </summary>
        public static Function SumNoneLoopLocal(Win64Container container, int count)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var def = new FunctionDefinition("main", new List<BaseType>(), intType);

            var instructions = new List<Instruction>();

            for (int i = 1; i <= count; i++)
            {
                instructions.Add(new Instruction(OpCodes.LoadInt, i));
            }

            for (int i = 0; i < count - 1; i++)
            {
                instructions.Add(new Instruction(OpCodes.AddInt));
            }

            instructions.Add(new Instruction(OpCodes.StoreLocal, 0));
            instructions.Add(new Instruction(OpCodes.LoadLocal, 0));
            instructions.Add(new Instruction(OpCodes.Return));

            return new Function(def, instructions, new List<BaseType>() { intType });
        }

        /// <summary>
        /// Creates a main function that calls the add function with the given number of arguments
        /// </summary>
        /// <param name="container">The container</param>
        /// <param name="numArgs">The number of arguments</param>
        public static Function AddMainFunction(Win64Container container, int numArgs)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
            var def = new FunctionDefinition("main", new List<BaseType>(), intType);

            var parameters = new List<BaseType>();
            for (int i = 0; i < numArgs; i++)
            {
                parameters.Add(intType);
            }

            var instructions = new List<Instruction>();

            for (int i = 1; i <= numArgs; i++)
            {
                instructions.Add(new Instruction(OpCodes.LoadInt, i));
            }

            instructions.Add(new Instruction(OpCodes.Call, "add", parameters));
            instructions.Add(new Instruction(OpCodes.Return));

            return new Function(def, instructions, new List<BaseType>());
        }

        /// <summary>
        /// Creates a add function with that takes the given amount of arguments
        /// </summary>
        /// <param name="container">The container</param>
        /// <param name="numArgs">The number of arguments</param>
        public static Function AddFunction(Win64Container container, int numArgs)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var parameters = new List<BaseType>();
            for (int i = 0; i < numArgs; i++)
            {
                parameters.Add(intType);
            }

            var def = new FunctionDefinition("add", parameters, intType);

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
        /// Creates a main function that calls the add function with the given number of arguments
        /// </summary>
        /// <param name="container">The container</param>
        /// <param name="numArgs">The number of arguments</param>
        public static Function FloatAddMainFunction(Win64Container container, int numArgs)
        {
            var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
            var def = new FunctionDefinition("floatMain", new List<BaseType>(), floatType);

            var parameters = new List<BaseType>();
            for (int i = 0; i < numArgs; i++)
            {
                parameters.Add(floatType);
            }

            var instructions = new List<Instruction>();

            for (int i = 1; i <= numArgs; i++)
            {
                instructions.Add(new Instruction(OpCodes.LoadFloat, (float)i));
            }

            instructions.Add(new Instruction(OpCodes.Call, "add", parameters));
            instructions.Add(new Instruction(OpCodes.Return));

            return new Function(def, instructions, new List<BaseType>());
        }

        /// <summary>
        /// Creates a add function with that takes the given amount of arguments
        /// </summary>
        /// <param name="container">The container</param>
        /// <param name="numArgs">The number of arguments</param>
        public static Function FloatAddFunction(Win64Container container, int numArgs)
        {
            var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);

            var parameters = new List<BaseType>();
            for (int i = 0; i < numArgs; i++)
            {
                parameters.Add(floatType);
            }

            var def = new FunctionDefinition("add", parameters, floatType);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadArgument, 0)
            };
            for (int i = 1; i < numArgs; i++)
            {
                instructions.Add(new Instruction(OpCodes.LoadArgument, i));
                instructions.Add(new Instruction(OpCodes.AddFloat));
            }

            instructions.Add(new Instruction(OpCodes.Return));

            return new Function(def, instructions, new List<BaseType>());
        }

        /// <summary>
        ///  Creates a recursive fibonacci function
        /// </summary>
        public static Function RecursiveFib(Win64Container container)
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
        /// Creates a recursive sum function
        /// </summary>
        public static Function ResursiveSum(Win64Container container)
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
        /// Creates the main function that invokes the given int function
        /// </summary>
        public static Function MainWithIntCall(Win64Container container, string toCall, int n)
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

        delegate float FloatEntryPoint();

        /// <summary>
        /// Executes a program that has an entry point that returns a float
        /// </summary>
        public static float ExecuteFloatProgram(Win64Container container, string entryPointName = "floatMain", string saveFileName = "")
        {
            container.VirtualMachine.Compile();

            if (saveFileName != "")
            {
                TestHelpers.SaveDisassembledFunctions(container, saveFileName);
            }

            var entryPoint = container.VirtualMachine.Binder.GetFunction(entryPointName + "()");
            var programPtr = (FloatEntryPoint)Marshal.GetDelegateForFunctionPointer(
                entryPoint.EntryPoint,
                typeof(FloatEntryPoint));

            return programPtr();
        }
    }
}
