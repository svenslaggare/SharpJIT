using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Core;
using SharpJIT.Core.Objects;
using SharpJIT.Runtime;

namespace SharpJIT.Test.GC
{
    /// <summary>
    /// Tests the garbage collector
    /// </summary>
    [TestClass]
    public class TestGC
    {
        /// <summary>
        /// Creates a VM container used for tests
        /// </summary>
        private Win64Container CreateContainer()
        {
            return new Win64Container(new VirtualMachineConfiguration(
                logAllocation: true,
                logDeallocation: true));
        }

        /// <summary>
        /// Tests deallocations for objects on the stack
        /// </summary>
        [TestMethod]
        public void TestDeallocationStack()
        {
            using (var container = this.CreateContainer())
            {
                var gc = container.VirtualMachine.GarbageCollector;

                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.Pop),
                        new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor
                });

                var result = container.Execute();
                Assert.AreEqual(1, gc.Deallocations.Count);
                Assert.AreEqual(1, gc.Deallocations[0].Count);
                Assert.AreEqual(gc.Allocations[0], gc.Deallocations[0][0]);
            }
        }

        /// <summary>
        /// Tests deallocations for objects on the stack
        /// </summary>
        [TestMethod]
        public void TestDeallocationStack2()
        {
            using (var container = this.CreateContainer())
            {
                var gc = container.VirtualMachine.GarbageCollector;

                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() {},
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
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
                Assert.AreEqual(1, gc.Deallocations.Count);
                Assert.AreEqual(0, gc.Deallocations[0].Count);
            }
        }

        /// <summary>
        /// Tests deallocations for objects stored as locals
        /// </summary>
        [TestMethod]
        public void TestDeallocationLocals()
        {
            using (var container = this.CreateContainer())
            {
                var gc = container.VirtualMachine.GarbageCollector;

                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { pointType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor
                });

                var result = container.Execute();
                Assert.AreEqual(1, gc.Deallocations.Count);
                Assert.AreEqual(0, gc.Deallocations[0].Count);
            }
        }

        /// <summary>
        /// Tests deallocations for objects stored as array elements
        /// </summary>
        [TestMethod]
        public void TestDeallocationArrayElements()
        {
            using (var container = this.CreateContainer())
            {
                var gc = container.VirtualMachine.GarbageCollector;

                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);
                var pointArrayType = container.VirtualMachine.TypeProvider.FindArrayType(pointType);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { pointArrayType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 10),
                        new Instruction(OpCodes.NewArray, pointType.Name),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreElement, pointType.Name),
                        new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor
                });

                var result = container.Execute();
                Assert.AreEqual(1, gc.Deallocations.Count);
                Assert.AreEqual(0, gc.Deallocations[0].Count);
            }
        }

        /// <summary>
        /// Tests deallocations for objects stored as array elements
        /// </summary>
        [TestMethod]
        public void TestDeallocationArrayElements2()
        {
            using (var container = this.CreateContainer())
            {
                var gc = container.VirtualMachine.GarbageCollector;

                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);
                var pointArrayType = container.VirtualMachine.TypeProvider.FindArrayType(pointType);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { pointArrayType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.LoadInt, 10),
                        new Instruction(OpCodes.NewArray, pointType.Name),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreElement, pointType.Name),
                        new Instruction(OpCodes.LoadNull),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor
                });

                var result = container.Execute();
                Assert.AreEqual(1, gc.Deallocations.Count);
                Assert.AreEqual(2, gc.Deallocations[0].Count);
                Assert.AreEqual(gc.Allocations[0], gc.Deallocations[0][0]);
                Assert.AreEqual(gc.Allocations[1], gc.Deallocations[0][1]);
            }
        }

        /// <summary>
        /// Tests deallocations for objects stored as fields
        /// </summary>
        [TestMethod]
        public void TestDeallocationClassFields()
        {
            using (var container = this.CreateContainer())
            {
                var gc = container.VirtualMachine.GarbageCollector;

                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);

                var listMetadata = new ClassMetadata("List");
                listMetadata.DefineField(new FieldDefinition("head", pointType, AccessModifier.Public));
                listMetadata.CreateFields();
                container.VirtualMachine.ClassMetadataProvider.Add(listMetadata);
                var listType = container.VirtualMachine.TypeProvider.FindClassType("List");

                var listConstructorFunction = TestHelpers.CreateDefaultConstructor(container.VirtualMachine, listType);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { listType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", listType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreField, "List::head"),
                        new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor,
                    listConstructorFunction
                });

                var result = container.Execute();
                Assert.AreEqual(1, gc.Deallocations.Count);
                Assert.AreEqual(0, gc.Deallocations[0].Count);
            }
        }

        /// <summary>
        /// Tests deallocations for objects stored as fields
        /// </summary>
        [TestMethod]
        public void TestDeallocationClassFields2()
        {
            using (var container = this.CreateContainer())
            {
                var gc = container.VirtualMachine.GarbageCollector;

                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);

                var listMetadata = new ClassMetadata("List");
                listMetadata.DefineField(new FieldDefinition("head", pointType, AccessModifier.Public));
                listMetadata.CreateFields();
                container.VirtualMachine.ClassMetadataProvider.Add(listMetadata);
                var listType = container.VirtualMachine.TypeProvider.FindClassType("List");

                var listConstructorFunction = TestHelpers.CreateDefaultConstructor(container.VirtualMachine, listType);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { listType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", listType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreField, "List::head"),
                        new Instruction(OpCodes.LoadNull),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor,
                    listConstructorFunction
                });

                var result = container.Execute();
                Assert.AreEqual(1, gc.Deallocations.Count);
                Assert.AreEqual(2, gc.Deallocations[0].Count);
                Assert.AreEqual(gc.Allocations[0], gc.Deallocations[0][0]);
                Assert.AreEqual(gc.Allocations[1], gc.Deallocations[0][1]);
            }
        }

        /// <summary>
        /// Tests deallocations for objects stored as fields
        /// </summary>
        [TestMethod]
        public void TestDeallocationClassFields3()
        {
            using (var container = this.CreateContainer())
            {
                var gc = container.VirtualMachine.GarbageCollector;

                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                (var pointType, var pointConstructor) = TestHelpers.DefinePointClass(container.VirtualMachine);

                var listMetadata = new ClassMetadata("List");
                listMetadata.DefineField(new FieldDefinition("head", pointType, AccessModifier.Public));
                listMetadata.CreateFields();
                container.VirtualMachine.ClassMetadataProvider.Add(listMetadata);
                var listType = container.VirtualMachine.TypeProvider.FindClassType("List");

                var listConstructorFunction = TestHelpers.CreateDefaultConstructor(container.VirtualMachine, listType);

                var func = new ManagedFunction(
                    new FunctionDefinition("main", new List<BaseType>(), intType),
                    new List<BaseType>() { listType },
                    new List<Instruction>()
                    {
                        new Instruction(OpCodes.NewObject, ".constructor", listType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.LoadLocal, 0),
                        new Instruction(OpCodes.NewObject, ".constructor", pointType, new List<BaseType>()),
                        new Instruction(OpCodes.StoreField, "List::head"),
                        new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
                        new Instruction(OpCodes.LoadNull),
                        new Instruction(OpCodes.StoreLocal, 0),
                        new Instruction(OpCodes.Call, "std.gc.collect", new List<BaseType>()),
                        new Instruction(OpCodes.LoadInt, 0),
                        new Instruction(OpCodes.Return)
                    });

                container.VirtualMachine.LoadFunctionsAsAssembly(new List<ManagedFunction>()
                {
                    func,
                    pointConstructor,
                    listConstructorFunction
                });

                var result = container.Execute();
                Assert.AreEqual(2, gc.Deallocations.Count);

                Assert.AreEqual(0, gc.Deallocations[0].Count);

                Assert.AreEqual(2, gc.Deallocations[1].Count);
                Assert.AreEqual(gc.Allocations[0], gc.Deallocations[1][0]);
                Assert.AreEqual(gc.Allocations[1], gc.Deallocations[1][1]);
            }
        }

    }
}
