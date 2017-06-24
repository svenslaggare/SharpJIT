using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Core;
using SharpJIT.Core.Objects;
using SharpJIT.Runtime;

namespace SharpJIT.Test.Programs
{
    /// <summary>
    /// Tests class instructions
    /// </summary>
    [TestClass]
    public class TestClass
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
        /// Defines the point class
        /// </summary>
        /// <param name="virtualMachine">The VM to define for</param>
        private (ClassType, ManagedFunction) DefinePointClass(VirtualMachine virtualMachine)
        {
            var intType = virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
            var voidType = virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Void);

            var pointMetadata = new ClassMetadata("Point");
            pointMetadata.DefineField(new FieldDefinition("x", intType, AccessModifier.Public));
            pointMetadata.DefineField(new FieldDefinition("y", intType, AccessModifier.Public));
            pointMetadata.CreateFields();

            virtualMachine.ClassMetadataProvider.Add(pointMetadata);

            var pointType = virtualMachine.TypeProvider.FindClassType("Point");

            var constructorFunction = new ManagedFunction(
                new FunctionDefinition(".constructor", new List<BaseType>(), voidType, pointType, true),
                new List<BaseType>(),
                new List<Instruction>()
                {
                    new Instruction(OpCodes.Return)
                });

            return (pointType, constructorFunction);
        }

        /// <summary>
        /// Tests the new object instruction
        /// </summary>
        [TestMethod]
        public void TestNewObject()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = this.DefinePointClass(container.VirtualMachine);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.Pop),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                var assembly = new Assembly("test", func, pointConstructor);

                container.VirtualMachine.LoadAssembly(assembly);
                var result = container.Execute();
                Assert.AreEqual(0, result);
            }
        }

        /// <summary>
        /// Tests the load field instruction
        /// </summary>
        [TestMethod]
        public void TestLoadField()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = this.DefinePointClass(container.VirtualMachine);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.LoadField, "Point::x"),
                        new Instruction(OpCodes.Return)
                    });

                var assembly = new Assembly("test", func, pointConstructor);

                container.VirtualMachine.LoadAssembly(assembly);
                var result = container.Execute();
                Assert.AreEqual(0, result);
            }
        }

        /// <summary>
        /// Tests the store field instruction
        /// </summary>
        [TestMethod]
        public void TestStoreField()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = this.DefinePointClass(container.VirtualMachine);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { pointType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadInt, 1337),
                        new Instruction(OpCodes.StoreField, "Point::x"),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadField, "Point::x"),
                        new Instruction(OpCodes.Return)
                    });

                var assembly = new Assembly("test", func, pointConstructor);

                container.VirtualMachine.LoadAssembly(assembly);
                var result = container.Execute();
                Assert.AreEqual(1337, result);
            }
        }

        /// <summary>
        /// Tests the store field instruction
        /// </summary>
        [TestMethod]
        public void TestStoreField2()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = this.DefinePointClass(container.VirtualMachine);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { pointType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadInt, 1337),
                        new Instruction(OpCodes.StoreField, "Point::y"),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadField, "Point::y"),
                        new Instruction(OpCodes.Return)
                    });

                var assembly = new Assembly("test", func, pointConstructor);

                container.VirtualMachine.LoadAssembly(assembly);
                var result = container.Execute();
                Assert.AreEqual(1337, result);
            }
        }

        /// <summary>
        /// Tests the call instance instruction
        /// </summary>
        [TestMethod]
        public void TestCallInstance()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = this.DefinePointClass(container.VirtualMachine);

                var pointAddFunc = new ManagedFunction(
                    new FunctionDefinition("add", new List<BaseType>(), intType, pointType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadArgument, 0),
                        new Instruction(OpCodes.LoadField, "Point::x"),
                        new Instruction(OpCodes.LoadArgument, 0),
                        new Instruction(OpCodes.LoadField, "Point::y"),
                        new Instruction(OpCodes.AddInt),
                        new Instruction(OpCodes.Return)
                    });

                var mainFunc = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { pointType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadInt, 4711),
                        new Instruction(OpCodes.StoreField, "Point::x"),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadInt, 1337),
                        new Instruction(OpCodes.StoreField, "Point::y"),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.CallInstance, "add", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.Return)
                    });

                var assembly = new Assembly("test", pointConstructor, pointAddFunc, mainFunc);

                container.VirtualMachine.LoadAssembly(assembly);
                var result = container.Execute();
                Assert.AreEqual(1337 + 4711, result);
            }
        }
    }
}
