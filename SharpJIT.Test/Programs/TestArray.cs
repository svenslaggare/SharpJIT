using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Core;
using SharpJIT.Runtime;

namespace SharpJIT.Test.Programs
{
    /// <summary>
    /// Tests array instructions
    /// </summary>
    [TestClass]
    public class TestArray
    {
        private delegate int Main();

        /// <summary>
        /// Returns the main delegate
        /// </summary>
        /// <param name="container">The container</param>
        private static Main MainDelegate(Win64Container container)
        {
            return Marshal.GetDelegateForFunctionPointer<Main>(
                    container.VirtualMachine.Binder.GetFunction("main()").EntryPoint);
        }

        /// <summary>
        /// Tests creating an array
        /// </summary>
        [TestMethod]
        public void TestCreateArray()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 10),
                        new Instruction(OpCodes.NewArray, intType.Name),
                        new Instruction(OpCodes.Pop),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                var functions = TestHelpers.SingleFunction(func);

                container.VirtualMachine.LoadFunctionsAsAssembly(functions);
                var result = container.Execute();
                Assert.AreEqual(0, result);
            }
        }

        /// <summary>
        /// Tests creating an invalid array
        /// </summary>
        [TestMethod]
        //[ExpectedException(typeof(RuntimeException))]
        public void TestInvalidCreateArray()
        {
            //using (var container = new Win64Container())
            //{
            //    var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            //    var func = new Function(
            //        new FunctionDefinition("main", new List<BaseType>(), intType),
            //        new List<Instruction>()
            //        {
            //            new Instruction(OpCodes.LoadInt, -1),
            //            new Instruction(OpCodes.NewArray, intType.Name),
            //            new Instruction(OpCodes.Pop),
            //            new Instruction(OpCodes.LoadInt, 0),
            //            new Instruction(OpCodes.Return)
            //        },
            //        new List<BaseType>());

            //    var functions = TestHelpers.SingleFunction(func);

            //    container.VirtualMachine.LoadAssembly(assembly);
            //    container.Execute();
            //}
        }

        /// <summary>
        /// Tests loading the length of an array
        /// </summary>
        [TestMethod]
        public void TestLoadArrayLength()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 10),
                        new Instruction(OpCodes.NewArray, intType.Name),
                        new Instruction(OpCodes.LoadArrayLength),
                        new Instruction(OpCodes.Return)
                    });

                var functions = TestHelpers.SingleFunction(func);

                container.VirtualMachine.LoadFunctionsAsAssembly(functions);
                var result = container.Execute();
                Assert.AreEqual(10, result);
            }
        }

        /// <summary>
        /// Tests loading the length of a null array
        /// </summary>
        [TestMethod]
        //[ExpectedException(typeof(RuntimeException))]
        public void TestLoadArrayLengthNull()
        {
            //using (var container = new Win64Container())
            //{
            //    var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            //    var func = new Function(
            //        new FunctionDefinition("main", new List<BaseType>(), intType),
            //        new List<Instruction>()
            //        {
            //            new Instruction(OpCodes.LoadNull),
            //            new Instruction(OpCodes.LoadArrayLength),
            //            new Instruction(OpCodes.Return)
            //        },
            //        new List<BaseType>());

            //    var functions = TestHelpers.SingleFunction(func);

            //    container.VirtualMachine.LoadAssembly(assembly);
            //    Assert.AreEqual(0, container.Execute());
            //}
        }

        /// <summary>
        /// Tests stores an element
        /// </summary>
        [TestMethod]
        public void TestStoreElement()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 10),
                        new Instruction(OpCodes.NewArray, intType.Name),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.LoadInt, 1337),
                        new Instruction(OpCodes.StoreElement, intType.Name),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                var functions = TestHelpers.SingleFunction(func);

                container.VirtualMachine.LoadFunctionsAsAssembly(functions);
                var result = container.Execute();
                Assert.AreEqual(0, result);
            }
        }

        /// <summary>
        /// Tests loading an element
        /// </summary>
        [TestMethod]
        public void TestLoadElement()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 10),
                        new Instruction(OpCodes.NewArray, intType.Name),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.LoadElement, intType.Name),
                        new Instruction(OpCodes.Return)
                    });

                var functions = TestHelpers.SingleFunction(func);

                container.VirtualMachine.LoadFunctionsAsAssembly(functions);
                var result = container.Execute();
                Assert.AreEqual(0, result);
            }
        }

        /// <summary>
        /// Tests loading an element
        /// </summary>
        [TestMethod]
        public void TestLoadElement2()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var arrayIntType = container.VirtualMachine.TypeProvider.FindArrayType(intType);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { arrayIntType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 10),
                        new Instruction(OpCodes.NewArray, intType.Name),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.LoadInt, 1337),
                        new Instruction(OpCodes.StoreElement, intType.Name),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.LoadElement, intType.Name),
                        new Instruction(OpCodes.Return)
                    });

                var functions = TestHelpers.SingleFunction(func);

                container.VirtualMachine.LoadFunctionsAsAssembly(functions);
                var result = container.Execute();
                Assert.AreEqual(1337, result);
            }
        }

        /// <summary>
        /// Tests loading an element
        /// </summary>
        [TestMethod]
        public void TestLoadElement3()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var arrayIntType = container.VirtualMachine.TypeProvider.FindArrayType(intType);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { arrayIntType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 10),
                        new Instruction(OpCodes.NewArray, intType.Name),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadInt, 4),
                        new Instruction(OpCodes.LoadInt, 1337),
                        new Instruction(OpCodes.StoreElement, intType.Name),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadInt, 4),
                        new Instruction(OpCodes.LoadElement, intType.Name),
                        new Instruction(OpCodes.Return)
                    });

                var functions = TestHelpers.SingleFunction(func);

                container.VirtualMachine.LoadFunctionsAsAssembly(functions);
                var result = container.Execute();
                Assert.AreEqual(1337, result);
            }
        }
    }
}
