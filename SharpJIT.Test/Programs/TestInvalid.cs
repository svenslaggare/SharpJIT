using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT;
using SharpJIT.Core;
using SharpJIT.Loader;

namespace SharpJIT.Test.Programs
{
    /// <summary>
    /// Tests programs that are invalid (e.g don't get pass verifier)
    /// </summary>
    [TestClass]
    public class TestInvalid
    {
        /// <summary>
        /// Tests an empty function
        /// </summary>
        [TestMethod]
        public void TestEmpty()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>(),
                    new List<Instruction>());

                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(func));

                try
                {
                    container.Execute();
                    Assert.Fail("Expected empty functions to not pass.");
                }
                catch (VerificationException e)
                {
                    Assert.AreEqual("0: Empty functions are not allowed.", e.Message);
                }
            }
        }

        /// <summary>
        /// Tests with a void parameter
        /// </summary>
        [TestMethod]
        public void TestVoidParameter()
        {
            using (var container = new Win64Container())
            {
                var voidType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Void);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.Return)
                };
                var func = new ManagedFunction(
                    new FunctionDefinition("test", new List<BaseType>() { voidType }, voidType),
                    new List<BaseType>(),
                    instructions);

                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(func));

                try
                {
                    container.Execute();
                    Assert.Fail("Expected void parameter to not pass.");
                }
                catch (VerificationException e)
                {
                    Assert.AreEqual("0: 'Void' is not a valid parameter type.", e.Message);
                }
            }
        }

        /// <summary>
        /// Tests ending a function without a return
        /// </summary>
        [TestMethod]
        public void TestNotEndInReturn()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 0)
                };
                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>(),
                    instructions);

                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(func));

                try
                {
                    container.Execute();
                    Assert.Fail("Expected without return to not pass.");
                }
                catch (VerificationException e)
                {
                    Assert.AreEqual("0: Functions must end with a 'RET' instruction.", e.Message);
                }
            }
        }
    }
}
