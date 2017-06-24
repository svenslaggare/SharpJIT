using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT;
using SharpJIT.Core;

namespace SharpJIT.Test.Programs
{
    /// <summary>
    /// Tests function definitions
    /// </summary>
    [TestClass]
    public class TestFunctions
    {
        /// <summary>
        /// Tests when the entry point is not defined
        /// </summary>
        [TestMethod]
        public void TestNoMain()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

                var testFunc = new ManagedFunction(
                    new FunctionDefinition("test", new List<BaseType>() { intType }, intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(testFunc));

                try
                {
                    container.Execute();
                    Assert.Fail("Expected no entry point to not pass.");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("There is no entry point defined.", e.Message);
                }
            }
        }

        /// <summary>
        /// Tests an invalid main function
        /// </summary>
        [TestMethod]
        public void TestInvalidMain()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

                var mainFunc = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>() { intType }, intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                try
                {
                    container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(mainFunc));
                    Assert.Fail("Expected invalid main to not pass.");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Expected the main function to have the signature: 'main() Int'.", e.Message);
                }
            }
        }

        /// <summary>
        /// Tests an invalid main function
        /// </summary>
        [TestMethod]
        public void TestInvalidMain2()
        {
            using (var container = new Win64Container())
            {
                var voidType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Void);

                var mainFunc = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>() { }, voidType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.Return)
                    });

                try
                {
                    container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(mainFunc));
                    Assert.Fail("Expected invalid main to not pass.");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Expected the main function to have the signature: 'main() Int'.", e.Message);
                }
            }
        }

        /// <summary>
        /// Tests function overload
        /// </summary>
        [TestMethod]
        public void TestOverload()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);

                var func1 = new ManagedFunction(
                    new FunctionDefinition("test", new List<BaseType>() { intType }, intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                var func2 = new ManagedFunction(
                    new FunctionDefinition("test", new List<BaseType>() { floatType }, floatType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadFloat, 0.0f),
                        new Instruction(OpCodes.Return)
                    });

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>() { func1, func2 });
            }
        }

        /// <summary>
        /// Tests invalid function overload
        /// </summary>
        [TestMethod]
        public void TestInvalidOverload()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);

                var func1 = new ManagedFunction(
                    new FunctionDefinition("test", new List<BaseType>() { intType }, intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                var func2 = new ManagedFunction(
                    new FunctionDefinition("test", new List<BaseType>() { intType }, floatType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadFloat, 0.0f),
                        new Instruction(OpCodes.Return)
                    });

                try
                {
                    container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>() { func1, func2 });
                    Assert.Fail("Expected invalid overload to not pass.");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("The function 'test(Int) Float' is already defined.", e.Message);
                }
            }
        }

        /// <summary>
        /// Tests defining a function that already exists
        /// </summary>
        [TestMethod]
        public void TestAlreadyDefined()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

                var func1 = new ManagedFunction(
                    new FunctionDefinition("test", new List<BaseType>(), intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                var func2 = new ManagedFunction(
                    new FunctionDefinition("test", new List<BaseType>(), intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                try
                {
                    container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>() { func1, func2 });
                    Assert.Fail("Expected already defined to not pass.");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("The function 'test() Int' is already defined.", e.Message);
                }
            }
        }
    }
}
