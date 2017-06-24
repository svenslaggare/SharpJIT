using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Core;

namespace SharpJIT.Test.Programs
{
    /// <summary>
    /// Tests bool instructions
    /// </summary>
    [TestClass]
    public class TestBool
    {
        delegate bool BoolEntryPoint();

        /// <summary>
        /// Executes a program that has an entry point that returns a bool
        /// </summary>
        private static bool ExecuteBoolProgram(Win64Container container, string entryPointName = "boolMain")
        {
            container.VirtualMachine.Compile();
            var entryPoint = container.VirtualMachine.Binder.GetFunction(entryPointName + "()");
            var programPtr = (BoolEntryPoint)Marshal.GetDelegateForFunctionPointer(
                entryPoint.EntryPoint,
                typeof(BoolEntryPoint));

            return programPtr();
        }

        /// <summary>
        /// Tests the load true instruction
        /// </summary>
        [TestMethod]
        public void TestLoadTrue()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the load true instruction
        /// </summary>
        [TestMethod]
        public void TestFalseTrue()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFalse),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the and instruction
        /// </summary>
        [TestMethod]
        public void TestAnd()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFalse),
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.And),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the and instruction
        /// </summary>
        [TestMethod]
        public void TestAnd2()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFalse),
                    new Instruction(OpCodes.LoadFalse),
                    new Instruction(OpCodes.And),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the and instruction
        /// </summary>
        [TestMethod]
        public void TestAnd3()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.And),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the or instruction
        /// </summary>
        [TestMethod]
        public void TestOr()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.Or),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the or instruction
        /// </summary>
        [TestMethod]
        public void TestOr2()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFalse),
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.Or),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the or instruction
        /// </summary>
        [TestMethod]
        public void TestOr3()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.LoadFalse),
                    new Instruction(OpCodes.Or),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the or instruction
        /// </summary>
        [TestMethod]
        public void TestOr4()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFalse),
                    new Instruction(OpCodes.LoadFalse),
                    new Instruction(OpCodes.And),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the not instruction
        /// </summary>
        [TestMethod]
        public void TestNot()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.Not),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the not instruction
        /// </summary>
        [TestMethod]
        public void TestNot2()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFalse),
                    new Instruction(OpCodes.Not),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareEqual()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.CompareEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareEqual2()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.LoadFalse),
                    new Instruction(OpCodes.CompareEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareEqual3()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.LoadInt, 1),
                    new Instruction(OpCodes.CompareEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareEqual4()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.CompareEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareEqual5()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.5f),
                    new Instruction(OpCodes.LoadFloat, 1.1f),
                    new Instruction(OpCodes.CompareEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareEqual6()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.CompareEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareEqual7()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadNull),
                    new Instruction(OpCodes.LoadNull),
                    new Instruction(OpCodes.CompareEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareEqual8()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var intArrayType = container.VirtualMachine.TypeProvider.FindArrayType(intType);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 10),
                    new Instruction(OpCodes.NewArray, intType.Name),
                    new Instruction(OpCodes.StoreLocal, 0),
                    new Instruction(OpCodes.LoadLocal, 0),
                    new Instruction(OpCodes.LoadLocal, 0),
                    new Instruction(OpCodes.CompareEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>() { intArrayType }, instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareEqual9()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadNull),
                    new Instruction(OpCodes.LoadInt, 10),
                    new Instruction(OpCodes.NewArray, intType.Name),
                    new Instruction(OpCodes.CompareEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare not equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareNotEqual()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.CompareNotEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare not equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareNotEqual2()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadTrue),
                    new Instruction(OpCodes.LoadFalse),
                    new Instruction(OpCodes.CompareNotEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare not equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareNotEqual3()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.LoadInt, 1),
                    new Instruction(OpCodes.CompareNotEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare not equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareNotEqual4()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.CompareNotEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare not equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareNotEqual5()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.5f),
                    new Instruction(OpCodes.LoadFloat, 1.1f),
                    new Instruction(OpCodes.CompareNotEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare not equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareNotEqual6()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.CompareNotEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare not equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareNotEqual7()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadNull),
                    new Instruction(OpCodes.LoadNull),
                    new Instruction(OpCodes.CompareNotEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare not equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareNotEqual8()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var intArrayType = container.VirtualMachine.TypeProvider.FindArrayType(intType);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 10),
                    new Instruction(OpCodes.NewArray, intType.Name),
                    new Instruction(OpCodes.StoreLocal, 0),
                    new Instruction(OpCodes.LoadLocal, 0),
                    new Instruction(OpCodes.LoadLocal, 0),
                    new Instruction(OpCodes.CompareNotEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>() { intArrayType }, instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare not equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareNotEqual9()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadNull),
                    new Instruction(OpCodes.LoadInt, 10),
                    new Instruction(OpCodes.NewArray, intType.Name),
                    new Instruction(OpCodes.CompareNotEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare greater than instruction
        /// </summary>
        [TestMethod]
        public void TestCompareGreaterThan()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 5),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.CompareGreaterThan),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare greater than instruction
        /// </summary>
        [TestMethod]
        public void TestCompareGreaterThan2()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 1),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.CompareGreaterThan),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare greater than instruction
        /// </summary>
        [TestMethod]
        public void TestCompareGreaterThan3()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 5.0f),
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.CompareGreaterThan),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare greater than instruction
        /// </summary>
        [TestMethod]
        public void TestCompareGreaterThan4()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 1.0f),
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.CompareGreaterThan),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare greater than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareGreaterThanOrEqual()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 5),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.CompareGreaterThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare greater than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareGreaterThanOrEqual2()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 1),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.CompareGreaterThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare greater than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareGreaterThanOrEqual3()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.CompareGreaterThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare greater than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareGreaterThanOrEqual4()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 5.0f),
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.CompareGreaterThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare greater than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareGreaterThanOrEqual5()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 1.0f),
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.CompareGreaterThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare greater than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareGreaterThanOrEqual6()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.CompareGreaterThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare less than instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThan()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.LoadInt, 5),
                    new Instruction(OpCodes.CompareLessThan),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare less than instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThan2()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 5),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.CompareLessThan),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare less than instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThan3()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.CompareLessThan),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare less than instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThan4()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.LoadFloat, 5.0f),
                    new Instruction(OpCodes.CompareLessThan),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare less than instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThan5()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 5.0f),
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.CompareLessThan),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare less than instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThan6()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.CompareLessThan),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare less than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThanOrEqual()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.LoadInt, 5),
                    new Instruction(OpCodes.CompareLessThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare less than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThanOrEqual2()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 5),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.CompareLessThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare less than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThanOrEqual3()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.CompareLessThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }


        /// <summary>
        /// Tests the compare less than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThanOrEqual4()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.LoadFloat, 5.0f),
                    new Instruction(OpCodes.CompareLessThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare less than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThanOrEqual5()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 5.0f),
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.CompareLessThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(false, ExecuteBoolProgram(container));
            }
        }

        /// <summary>
        /// Tests the compare less than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestCompareLessThanOrEqual6()
        {
            using (var container = new Win64Container())
            {
                var boolType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
                var funcDef = new FunctionDefinition("boolMain", new List<BaseType>(), boolType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.LoadFloat, 2.0f),
                    new Instruction(OpCodes.CompareLessThanOrEqual),
                    new Instruction(OpCodes.Return)
                };

                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(true, ExecuteBoolProgram(container));
            }
        }
    }
}
