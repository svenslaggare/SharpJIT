﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Core;
using SharpJIT.Core.Objects;
using SharpJIT.Runtime;

namespace SharpJIT.Test.InstructionSet
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
        /// Tests the new object instruction
        /// </summary>
        [TestMethod]
        public void TestNewObject()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);

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

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor
                });

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
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.LoadField, "Point::x"),
                        new Instruction(OpCodes.Return)
                    });

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor
                });

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
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);

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

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor
                });

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
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);

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

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor
                });

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
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);

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

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    pointConstructor,
                    pointAddFunc,
                    mainFunc
                });

                var result = container.Execute();
                Assert.AreEqual(1337 + 4711, result);
            }
        }

        /// <summary>
        /// Tests constructors
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var voidType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Void);

                var pointMetadata = new ClassMetadata("Point");
                pointMetadata.DefineField(new FieldDefinition("x", intType, AccessModifier.Public));
                pointMetadata.DefineField(new FieldDefinition("y", intType, AccessModifier.Public));
                pointMetadata.CreateFields();

                container.VirtualMachine.ClassMetadataProvider.Add(pointMetadata);

                var pointType = container.VirtualMachine.TypeProvider.FindClassType("Point");

                var pointConstructor = new ManagedFunction(
                    new FunctionDefinition(".constructor", new List<BaseType>() { intType, intType }, voidType, pointType, true),
                    new List<BaseType>(),
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadArgument, 0),
                        new Instruction(OpCodes.LoadArgument, 1),
                        new Instruction(OpCodes.StoreField, "Point::x"),
                        new Instruction(OpCodes.LoadArgument, 0),
                        new Instruction(OpCodes.LoadArgument, 2),
                        new Instruction(OpCodes.StoreField, "Point::y"),
                        new Instruction(OpCodes.Return)
                    });

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { pointType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 1337),
                        new Instruction(OpCodes.LoadInt, 4711),
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>() { intType, intType }),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadField, "Point::x"),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadField, "Point::y"),
                        new Instruction(OpCodes.AddInt),
                        new Instruction(OpCodes.Return)
                    });

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor
                });

                var result = container.Execute();
                Assert.AreEqual(1337 + 4711, result);
            }
        }
    }
}
