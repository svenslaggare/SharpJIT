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
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<BaseType>(), intType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadInt, 100),
                    new Instruction(OpCodes.StoreLocal, 0),

                    new Instruction(OpCodes.LoadInt, 200),
                    new Instruction(OpCodes.StoreLocal, 1),

                    new Instruction(OpCodes.LoadInt, 300),
                    new Instruction(OpCodes.StoreLocal, 2),

                    new Instruction(OpCodes.LoadInt, 400),
                    new Instruction(OpCodes.StoreLocal, 3),

                    new Instruction(OpCodes.LoadLocal, 3),
                    new Instruction(OpCodes.Return)
                };
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
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var funcDef = new FunctionDefinition("main", new List<BaseType>(), intType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadLocal, 0),
                    new Instruction(OpCodes.Return)
                };
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
                var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
                var funcDef = new FunctionDefinition("floatMain", new List<BaseType>(), floatType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadLocal, 0),
                    new Instruction(OpCodes.Return)
                };
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
