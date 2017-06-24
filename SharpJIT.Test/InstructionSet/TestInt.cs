using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT;
using SharpJIT.Core;

namespace SharpJIT.Test.InstructionSet
{
    /// <summary>
    /// Test integer instructions
    /// </summary>
    [TestClass]
    public class TestInt
    {
        /// <summary>
        /// Tests the add instruction
        /// </summary>
        [TestMethod]
        public void TestAdd()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<BaseType>(), intType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.LoadInt, 1),
                    new Instruction(OpCodes.AddInt),
                    new Instruction(OpCodes.Return)
                };
                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(3, container.Execute());
            }
        }

        /// <summary>
        /// Tests the sub instruction
        /// </summary>
        [TestMethod]
        public void TestSub()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<BaseType>(), intType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 4),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.SubInt),
                    new Instruction(OpCodes.Return)
                };
                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(2, container.Execute());
            }
        }

        /// <summary>
        /// Tests the mul instruction
        /// </summary>
        [TestMethod]
        public void TestMul()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<BaseType>(), intType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.LoadInt, 3),
                    new Instruction(OpCodes.MulInt),
                    new Instruction(OpCodes.Return)
                };
                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(6, container.Execute());
            }
        }

        /// <summary>
        /// Tests the div instruction
        /// </summary>
        [TestMethod]
        public void TestDiv()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<BaseType>(), intType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 4),
                    new Instruction(OpCodes.LoadInt, 2),
                    new Instruction(OpCodes.DivInt),
                    new Instruction(OpCodes.Return)
                };
                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(4 / 2, container.Execute());
            }
        }
    }
}
