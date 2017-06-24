using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT;
using SharpJIT.Core;

namespace SharpJIT.Test.Programs
{
    /// <summary>
    /// Test branch instructions
    /// </summary>
    [TestClass]
    public class TestBranches
    {
        /// <summary>
        /// Creates a new branch program
        /// </summary>
        private ManagedFunction CreateBranchProgram(Win64Container container, OpCodes branchInstruction, int value1, int value2)
        {
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadInt, value1),
                new Instruction(OpCodes.LoadInt, value2),
                new Instruction(branchInstruction, 6),
                new Instruction(OpCodes.LoadInt, 0),
                new Instruction(OpCodes.StoreLocal, 0),
                new Instruction(OpCodes.Branch, 8),
                new Instruction(OpCodes.LoadInt, 1),
                new Instruction(OpCodes.StoreLocal, 0),
                new Instruction(OpCodes.LoadLocal, 0),
                new Instruction(OpCodes.Return)
            };
            return new ManagedFunction(
                new FunctionDefinition("main", new List<BaseType>(), intType),
                new List<BaseType>() { intType },
                instructions);
        }

        /// <summary>
        /// Creates a new float branch program
        /// </summary>
        private ManagedFunction CreateBranchFloatProgram(Win64Container container, OpCodes branchInstruction, float value1, float value2)
        {
            var floatType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
            var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var instructions = new List<Instruction>
            {
                new Instruction(OpCodes.LoadFloat, value1),
                new Instruction(OpCodes.LoadFloat, value2),
                new Instruction(branchInstruction, 6),
                new Instruction(OpCodes.LoadInt, 0),
                new Instruction(OpCodes.StoreLocal, 0),
                new Instruction(OpCodes.Branch, 8),
                new Instruction(OpCodes.LoadInt, 1),
                new Instruction(OpCodes.StoreLocal, 0),
                new Instruction(OpCodes.LoadLocal, 0),
                new Instruction(OpCodes.Return)
            };
            return new ManagedFunction(
                new FunctionDefinition("main", new List<BaseType>(), intType),
                new List<BaseType>() { intType },
                instructions);
        }

        /// <summary>
        /// Tests the branch equal instruction
        /// </summary>
        [TestMethod]
        public void TestBranchEqual()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchEqual, 1, 1)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchEqual, 2, 1)));

                Assert.AreEqual(0, container.Execute());
            }
        }

        /// <summary>
        /// Tests the branch not equal instruction
        /// </summary>
        [TestMethod]
        public void TestBranchNotEqual()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchNotEqual, 2, 1)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchNotEqual, 1, 1)));

                Assert.AreEqual(0, container.Execute());
            }
        }

        /// <summary>
        /// Tests the branch less than instruction
        /// </summary>
        [TestMethod]
        public void TestLessThan()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchLessThan, 1, 2)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchLessThan, 1, 1)));

                Assert.AreEqual(0, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchLessThan, 2, 1)));

                Assert.AreEqual(0, container.Execute());
            }
        }

        /// <summary>
        /// Tests the branch less than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestLessThanOrEqual()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchLessOrEqual, 1, 2)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchLessOrEqual, 1, 1)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchLessOrEqual, 2, 1)));

                Assert.AreEqual(0, container.Execute());
            }
        }

        /// <summary>
        /// Tests the branch grater than instruction
        /// </summary>
        [TestMethod]
        public void TestGreaterThan()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchGreaterThan, 2, 1)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchGreaterThan, 1, 1)));

                Assert.AreEqual(0, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchGreaterThan, 1, 2)));

                Assert.AreEqual(0, container.Execute());
            }
        }

        /// <summary>
        /// Tests the branch grater than or equal instruction
        /// </summary>
        [TestMethod]
        public void TestGreaterThanOrEqual()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchGreaterThanOrEqual, 2, 1)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchGreaterThanOrEqual, 1, 1)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchProgram(container, OpCodes.BranchGreaterThanOrEqual, 1, 2)));

                Assert.AreEqual(0, container.Execute());
            }
        }

        /// <summary>
        /// Tests the branch equal instruction float
        /// </summary>
        [TestMethod]
        public void TestBranchEqualFloat()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchEqual, 1.0f, 1.0f)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchEqual, 2.0f, 1.0f)));

                Assert.AreEqual(0, container.Execute());
            }
        }

        /// <summary>
        /// Tests the branch not equal instruction float
        /// </summary>
        [TestMethod]
        public void TestBranchNotEqualFloat()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchNotEqual, 2, 1)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchNotEqual, 1, 1)));

                Assert.AreEqual(0, container.Execute());
            }
        }

        /// <summary>
        /// Tests the branch less than instruction float
        /// </summary>
        [TestMethod]
        public void TestLessThanFloat()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchLessThan, 1, 2)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchLessThan, 1, 1)));

                Assert.AreEqual(0, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchLessThan, 2, 1)));

                Assert.AreEqual(0, container.Execute());
            }
        }

        /// <summary>
        /// Tests the branch less than or equal instruction float
        /// </summary>
        [TestMethod]
        public void TestLessThanOrEqualFloat()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchLessOrEqual, 1, 2)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchLessOrEqual, 1, 1)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchLessOrEqual, 2, 1)));

                Assert.AreEqual(0, container.Execute());
            }
        }

        /// <summary>
        /// Tests the branch grater than instruction float
        /// </summary>
        [TestMethod]
        public void TestGreaterThanFloat()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchGreaterThan, 2, 1)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchGreaterThan, 1, 1)));

                Assert.AreEqual(0, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchGreaterThan, 1, 2)));

                Assert.AreEqual(0, container.Execute());
            }
        }

        /// <summary>
        /// Tests the branch grater than or equal instruction float
        /// </summary>
        [TestMethod]
        public void TestGreaterThanOrEqualFloat()
        {
            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchGreaterThanOrEqual, 2, 1)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchGreaterThanOrEqual, 1, 1)));

                Assert.AreEqual(1, container.Execute());
            }

            using (var container = new Win64Container())
            {
                container.VirtualMachine.LoadAssemblyInternal(Assembly.SingleFunction(
                    this.CreateBranchFloatProgram(container, OpCodes.BranchGreaterThanOrEqual, 1, 2)));

                Assert.AreEqual(0, container.Execute());
            }
        }
    }
}
