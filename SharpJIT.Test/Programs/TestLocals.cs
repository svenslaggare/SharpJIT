using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT;
using SharpJIT.Core;

namespace SharpJIT.Test.Programs
{
    /// <summary>
    /// Tests locals
    /// </summary>
    [TestClass]
    public class TestLocals
    {
        /// <summary>
        /// Tests locals
        /// </summary>
        [TestMethod]
        public void TestLocals1()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.GetPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<VMType>(), intType);

                var instructions = new List<Instruction>();

                instructions.Add(new Instruction(OpCodes.LoadInt, 100));
                instructions.Add(new Instruction(OpCodes.StoreLocal, 0));

                instructions.Add(new Instruction(OpCodes.LoadInt, 200));
                instructions.Add(new Instruction(OpCodes.StoreLocal, 1));

                instructions.Add(new Instruction(OpCodes.LoadInt, 300));
                instructions.Add(new Instruction(OpCodes.StoreLocal, 2));

                instructions.Add(new Instruction(OpCodes.LoadInt, 400));
                instructions.Add(new Instruction(OpCodes.StoreLocal, 3));

                instructions.Add(new Instruction(OpCodes.LoadLocal, 3));
                instructions.Add(new Instruction(OpCodes.Ret));

                var func = new Function(funcDef, instructions, Enumerable.Repeat(intType, 4).ToList());
                container.LoadAssembly(Assembly.SingleFunction(func));
                Assert.AreEqual(400, container.Execute());
            }
        }

        /// <summary>
        /// Tests default values for int locals
        /// </summary>
        [TestMethod]
        public void TestIntDefaultValue()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.GetPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<VMType>(), intType);

                var instructions = new List<Instruction>();

                instructions.Add(new Instruction(OpCodes.LoadLocal, 0));
                instructions.Add(new Instruction(OpCodes.Ret));

                var func = new Function(funcDef, instructions, Enumerable.Repeat(intType, 1).ToList());
                container.LoadAssembly(Assembly.SingleFunction(func));
                Assert.AreEqual(0, container.Execute());
            }
        }

        private delegate float FloatMain();

        /// <summary>
        /// Tests default values for float locals
        /// </summary>
        [TestMethod]
        public void TestFloatDefaultValue()
        {
            using (var container = new Win64Container())
            {
                var floatType = container.VirtualMachine.TypeProvider.GetPrimitiveType(PrimitiveTypes.Float);
                var funcDef = new FunctionDefinition("floatMain", new List<VMType>(), floatType);

                var instructions = new List<Instruction>();

                instructions.Add(new Instruction(OpCodes.LoadLocal, 0));
                instructions.Add(new Instruction(OpCodes.Ret));

                var func = new Function(funcDef, instructions, Enumerable.Repeat(floatType, 1).ToList());
                container.LoadAssembly(Assembly.SingleFunction(func));
                container.VirtualMachine.Compile();
                var mainFunc = Marshal.GetDelegateForFunctionPointer<FloatMain>(
                    container.VirtualMachine.Binder.GetFunction("floatMain()").EntryPoint);

                Assert.AreEqual(0.0f, mainFunc());
            }
        }
    }
}
