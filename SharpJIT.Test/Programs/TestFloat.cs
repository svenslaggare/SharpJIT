using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT;
using SharpJIT.Core;

namespace SharpJIT.Test.Programs
{
    /// <summary>
    /// Tests floating point instructions
    /// </summary>
    [TestClass]
    public class TestFloat
    {
        delegate float FloatEntryPoint();

        /// <summary>
        /// Executes a program that has an entry point that returns a float
        /// </summary>
        private static float ExecuteFloatProgram(Win64Container container, string entryPointName = "floatMain")
        {
            container.VirtualMachine.Compile();
            var entryPoint = container.VirtualMachine.Binder.GetFunction(entryPointName + "()");
            var programPtr = (FloatEntryPoint)Marshal.GetDelegateForFunctionPointer(
                entryPoint.EntryPoint,
                typeof(FloatEntryPoint));

            return programPtr();
        }

        /// <summary>
        /// Tests the add instruction
        /// </summary>
        [TestMethod]
        public void TestAdd()
        {
            using (var container = new Win64Container())
            {
                var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
                var funcDef = new FunctionDefinition("floatMain", new List<BaseType>(), floatType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.5f),
                    new Instruction(OpCodes.LoadFloat, 1.35f),
                    new Instruction(OpCodes.AddFloat),
                    new Instruction(OpCodes.Return)
                };
                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(2.5f + 1.35f, ExecuteFloatProgram(container), 1E-4);
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
                var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
                var funcDef = new FunctionDefinition("floatMain", new List<BaseType>(), floatType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.5f),
                    new Instruction(OpCodes.LoadFloat, 1.35f),
                    new Instruction(OpCodes.SubFloat),
                    new Instruction(OpCodes.Return)
                };
                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(2.5f - 1.35f, ExecuteFloatProgram(container), 1E-4);
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
                var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
                var funcDef = new FunctionDefinition("floatMain", new List<BaseType>(), floatType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.5f),
                    new Instruction(OpCodes.LoadFloat, 1.35f),
                    new Instruction(OpCodes.MulFloat),
                    new Instruction(OpCodes.Return)
                };
                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(2.5f * 1.35f, ExecuteFloatProgram(container), 1E-4);
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
                var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
                var funcDef = new FunctionDefinition("floatMain", new List<BaseType>(), floatType);

                var instructions = new List<Instruction>
                {
                    new Instruction(OpCodes.LoadFloat, 2.5f),
                    new Instruction(OpCodes.LoadFloat, 1.35f),
                    new Instruction(OpCodes.DivFloat),
                    new Instruction(OpCodes.Return)
                };
                var func = new ManagedFunction(funcDef, new List<BaseType>(), instructions);
                container.VirtualMachine.LoadFunctionsAsAssembly(TestHelpers.SingleFunction(func));
                Assert.AreEqual(2.5f / 1.35f, ExecuteFloatProgram(container), 1E-4);
            }
        }
    }
}
