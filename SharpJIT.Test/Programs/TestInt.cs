using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT;
using SharpJIT.Core;

namespace SharpJIT.Test.Programs
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
                var intType = container.VirtualMachine.TypeProvider.GetPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<VMType>(), intType);

                var instructions = new List<Instruction>();

                instructions.Add(new Instruction(OpCodes.LoadInt, 2));
                instructions.Add(new Instruction(OpCodes.LoadInt, 1));
                instructions.Add(new Instruction(OpCodes.AddInt));
                instructions.Add(new Instruction(OpCodes.Ret));

                var func = new Function(funcDef, instructions, new List<VMType>());
                container.LoadAssembly(Assembly.SingleFunction(func));
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
                var intType = container.VirtualMachine.TypeProvider.GetPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<VMType>(), intType);

                var instructions = new List<Instruction>();

                instructions.Add(new Instruction(OpCodes.LoadInt, 4));
                instructions.Add(new Instruction(OpCodes.LoadInt, 2));
                instructions.Add(new Instruction(OpCodes.SubInt));
                instructions.Add(new Instruction(OpCodes.Ret));

                var func = new Function(funcDef, instructions, new List<VMType>());
                container.LoadAssembly(Assembly.SingleFunction(func));
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
                var intType = container.VirtualMachine.TypeProvider.GetPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<VMType>(), intType);

                var instructions = new List<Instruction>();

                instructions.Add(new Instruction(OpCodes.LoadInt, 2));
                instructions.Add(new Instruction(OpCodes.LoadInt, 3));
                instructions.Add(new Instruction(OpCodes.MulInt));
                instructions.Add(new Instruction(OpCodes.Ret));

                var func = new Function(funcDef, instructions, new List<VMType>());
                container.LoadAssembly(Assembly.SingleFunction(func));
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
                var intType = container.VirtualMachine.TypeProvider.GetPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<VMType>(), intType);

                var instructions = new List<Instruction>();

                instructions.Add(new Instruction(OpCodes.LoadInt, 4));
                instructions.Add(new Instruction(OpCodes.LoadInt, 2));
                instructions.Add(new Instruction(OpCodes.DivInt));
                instructions.Add(new Instruction(OpCodes.Ret));

                var func = new Function(funcDef, instructions, new List<VMType>());
                container.LoadAssembly(Assembly.SingleFunction(func));
                Assert.AreEqual(4 / 2, container.Execute());
            }
        }
    }
}
