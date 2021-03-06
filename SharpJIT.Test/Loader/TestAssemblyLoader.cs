﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Core;
using SharpJIT.Core.Objects;
using SharpJIT.Loader;
using SharpJIT.Runtime;

namespace SharpJIT.Test.Loader
{
    /// <summary>
    /// Tests the assembly loader
    /// </summary>
    [TestClass]
    public class TestAssemblyLoader
    {
        /// <summary>
        /// Tests loading a function
        /// </summary>
        [TestMethod]
        public void TestLoadFunction()
        {
            var classMetadataProvider = new ClassMetadataProvider();
            var typeProvider = new TypeProvider(classMetadataProvider);
            var functionLoader = new FunctionLoader(typeProvider);

            var intType = typeProvider.FindPrimitiveType(PrimitiveTypes.Int);
            var floatType = typeProvider.FindPrimitiveType(PrimitiveTypes.Float);
       
            var parsedFunction = new SharpJIT.Loader.Data.Function(
                "test",
                new List<string>() { intType.Name },
                intType.Name,
                new List<string>() { floatType.Name },
                new List<SharpJIT.Loader.Data.Instruction>()
                {
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadInt, 1337),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.Return)
                });

            var functionDefinition = new FunctionDefinition("test", new List<BaseType>() { intType }, intType);
            var function = functionLoader.LoadManagedFunction(parsedFunction, functionDefinition);

            Assert.AreEqual(functionDefinition, function.Definition);

            Assert.AreEqual(1, function.Locals.Count);
            Assert.AreEqual(floatType, function.Locals[0]);

            Assert.AreEqual(2, function.Instructions.Count);
            Assert.AreEqual(OpCodes.LoadInt, function.Instructions[0].OpCode);
            Assert.AreEqual(1337, function.Instructions[0].IntValue);
            Assert.AreEqual(OpCodes.Return, function.Instructions[1].OpCode);
        }

        /// <summary>
        /// Tests loading a class
        /// </summary>
        [TestMethod]
        public void TestClassLoader()
        {
            var classMetadataProvider = new ClassMetadataProvider();
            var typeProvider = new TypeProvider(classMetadataProvider);
            var classLoader = new ClassLoader(typeProvider, classMetadataProvider);

            var intType = typeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var classDef = new SharpJIT.Loader.Data.Class(
                "Point",
                new List<SharpJIT.Loader.Data.Field>()
                {
                    new SharpJIT.Loader.Data.Field("x", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public),
                    new SharpJIT.Loader.Data.Field("y", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public)
                });

            classLoader.LoadClasses(new List<SharpJIT.Loader.Data.Class>() { classDef });

            var classMetadata = classMetadataProvider.GetMetadata(classDef.Name);

            Assert.IsNotNull(classMetadata);
            Assert.AreEqual(classDef.Name, classMetadata.Name);

            var fields = classMetadata.Fields.ToList();
            Assert.AreEqual(2, fields.Count);
            Assert.AreEqual("x", fields[0].Name);
            Assert.AreEqual(intType, fields[0].Type);

            Assert.AreEqual("y", fields[1].Name);
            Assert.AreEqual(intType, fields[1].Type);
        }

        /// <summary>
        /// Tests loading a class
        /// </summary>
        [TestMethod]
        public void TestClassLoader2()
        {
            var classMetadataProvider = new ClassMetadataProvider();
            var typeProvider = new TypeProvider(classMetadataProvider);
            var classLoader = new ClassLoader(typeProvider, classMetadataProvider);

            var intType = typeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var listClassDef = new SharpJIT.Loader.Data.Class(
                "List",
                new List<SharpJIT.Loader.Data.Field>()
                {
                    new SharpJIT.Loader.Data.Field("head", "Ref.Point", SharpJIT.Core.Objects.AccessModifier.Public)
                });

            var pointClassDef = new SharpJIT.Loader.Data.Class(
                "Point",
                new List<SharpJIT.Loader.Data.Field>()
                {
                    new SharpJIT.Loader.Data.Field("x", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public),
                    new SharpJIT.Loader.Data.Field("y", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public)
                });

            classLoader.LoadClasses(new List<SharpJIT.Loader.Data.Class>() { listClassDef, pointClassDef });

            Assert.IsNotNull(classMetadataProvider.GetMetadata(pointClassDef.Name));

            var listMetadata = classMetadataProvider.GetMetadata(listClassDef.Name);
            Assert.IsNotNull(listMetadata);

            var pointType = typeProvider.FindType("Ref.Point", false);
            Assert.IsNotNull(pointType);

            Assert.AreEqual(pointType, listMetadata.Fields.First().Type);
        }

        /// <summary>
        /// Tests loading an assembly
        /// </summary>
        [TestMethod]
        public void TestLoadAssembly()
        {
            var classMetadataProvider = new ClassMetadataProvider();
            var typeProvider = new TypeProvider(classMetadataProvider);
            var assemblyLoader = new AssemblyLoader(
                new ClassLoader(typeProvider, classMetadataProvider),
                new FunctionLoader(typeProvider),
                typeProvider);

            var intType = typeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var classDef = new SharpJIT.Loader.Data.Class(
                "Point",
                new List<SharpJIT.Loader.Data.Field>()
                {
                    new SharpJIT.Loader.Data.Field("x", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public),
                    new SharpJIT.Loader.Data.Field("y", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public)
                });

            var functionDef = new SharpJIT.Loader.Data.Function(
                "test",
                new List<string>() { intType.Name },
                intType.Name,
                new List<string>() { "Ref.Point" },
                new List<SharpJIT.Loader.Data.Instruction>()
                {
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadInt, 1337),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.Return)
                });

            var assembly = new SharpJIT.Loader.Data.Assembly(
                "test",
                new List<SharpJIT.Loader.Data.Class>() { classDef },
                new List<SharpJIT.Loader.Data.Function>() { functionDef });

            var loadedAssembly = assemblyLoader.LoadAssembly(assembly);

            Assert.AreEqual(1, loadedAssembly.Functions.Count);
            Assert.AreEqual("test", loadedAssembly.Functions[0].Definition.Name);

            Assert.AreEqual(1, loadedAssembly.Classes.Count);
            Assert.IsTrue(classMetadataProvider.IsDefined("Point"));
        }

        /// <summary>
        /// Tests loading an assembly
        /// </summary>
        [TestMethod]
        public void TestLoadAssembly2()
        {
            var classMetadataProvider = new ClassMetadataProvider();
            var typeProvider = new TypeProvider(classMetadataProvider);
            var assemblyLoader = new AssemblyLoader(
                new ClassLoader(typeProvider, classMetadataProvider),
                new FunctionLoader(typeProvider),
                typeProvider);

            var intType = typeProvider.FindPrimitiveType(PrimitiveTypes.Int);

            var classDef = new SharpJIT.Loader.Data.Class(
                "Point",
                new List<SharpJIT.Loader.Data.Field>()
                {
                    new SharpJIT.Loader.Data.Field("x", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public),
                    new SharpJIT.Loader.Data.Field("y", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public)
                });

            var functionDef = new SharpJIT.Loader.Data.Function(
                "test",
                new List<string>() { intType.Name },
                intType.Name,
                "Point",
                false,
                new List<string>() { "Ref.Point" },
                new List<SharpJIT.Loader.Data.Instruction>()
                {
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadInt, 1337),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.Return)
                });

            var assembly = new SharpJIT.Loader.Data.Assembly(
                "test",
                new List<SharpJIT.Loader.Data.Class>() { classDef },
                new List<SharpJIT.Loader.Data.Function>() { functionDef });

            var loadedAssembly = assemblyLoader.LoadAssembly(assembly);

            Assert.AreEqual(1, loadedAssembly.Functions.Count);

            var loadedFunction = loadedAssembly.Functions[0];
            var pointType = typeProvider.FindClassType("Point");

            Assert.AreEqual(1, loadedAssembly.Classes.Count);

            Assert.IsNotNull(pointType);
            Assert.AreEqual("Point::test", loadedFunction.Definition.Name);
            Assert.AreEqual(pointType, loadedFunction.Definition.ClassType);
            Assert.AreEqual("test", loadedFunction.Definition.MemberName);

            Assert.IsTrue(classMetadataProvider.IsDefined("Point"));
        }

        /// <summary>
        /// Tests loading an assembly
        /// </summary>
        [TestMethod]
        public void TestLoadAssembly3()
        {
            var classMetadataProvider = new ClassMetadataProvider();
            var typeProvider = new TypeProvider(classMetadataProvider);
            var assemblyLoader = new AssemblyLoader(
                new ClassLoader(typeProvider, classMetadataProvider),
                new FunctionLoader(typeProvider),
                typeProvider);

            var intType = typeProvider.FindPrimitiveType(PrimitiveTypes.Int);
            var voidType = typeProvider.FindPrimitiveType(PrimitiveTypes.Void);

            var classDef = new SharpJIT.Loader.Data.Class(
                "Point",
                new List<SharpJIT.Loader.Data.Field>()
                {
                    new SharpJIT.Loader.Data.Field("x", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public),
                    new SharpJIT.Loader.Data.Field("y", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public)
                });

            var pointConstructor = new SharpJIT.Loader.Data.Function(
                ".constructor",
                new List<string>(),
                voidType.Name,
                "Point",
                true,
                new List<string>(),
                new List<SharpJIT.Loader.Data.Instruction>()
                {
                    new SharpJIT.Loader.Data.Instruction(OpCodes.Return)
                });

            var pointAddFunction = new SharpJIT.Loader.Data.Function(
                "add",
                new List<string>(),
                intType.Name,
                "Point",
                false,
                new List<string>(),
                new List<SharpJIT.Loader.Data.Instruction>()
                {
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadArgument, 0),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadField, "Point::x"),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadArgument, 0),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadField, "Point::y"),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.AddInt),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.Return)
                });

            var mainFunction = new SharpJIT.Loader.Data.Function(
                "main",
                new List<string>(),
                intType.Name,
                new List<string>() { "Ref.Point" },
                new List<SharpJIT.Loader.Data.Instruction>()
                {
                    new SharpJIT.Loader.Data.Instruction(OpCodes.NewObject, ".constructor", "Point", new List<string>()),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.StoreLocal, 0),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadLocal, 0),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadInt, 4711),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.StoreField, "Point::x"),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadLocal, 0),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadInt, 1337),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.StoreField, "Point::y"),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.LoadLocal, 0),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.CallInstance, "add", "Point", new List<string>()),
                    new SharpJIT.Loader.Data.Instruction(OpCodes.Return)
                });

            var assembly = new SharpJIT.Loader.Data.Assembly(
                "test",
                new List<SharpJIT.Loader.Data.Class>() { classDef },
                new List<SharpJIT.Loader.Data.Function>() { pointConstructor, pointAddFunction, mainFunction });

            var loadedAssembly = assemblyLoader.LoadAssembly(assembly);

            var loadedFunction = loadedAssembly.Functions.Last();
            var callInstanceInstruction = loadedFunction.Instructions[loadedFunction.Instructions.Count - 2];
            Assert.IsNotNull(callInstanceInstruction.ClassType);
        }

        /// <summary>
        /// Tests loading the assembly in the VM
        /// </summary>
        [TestMethod]
        public void TestLoadAssemblyVM()
        {
            using (var container = new Win64Container())
            {
                var intType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var voidType = container.VirtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Void);

                var classDef = new SharpJIT.Loader.Data.Class(
                    "Point",
                    new List<SharpJIT.Loader.Data.Field>()
                    {
                        new SharpJIT.Loader.Data.Field("x", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public),
                        new SharpJIT.Loader.Data.Field("y", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public)
                    });

                var pointConstructor = new SharpJIT.Loader.Data.Function(
                    ".constructor",
                    new List<string>(),
                    voidType.Name,
                    "Point",
                    true,
                    new List<string>(),
                    new List<SharpJIT.Loader.Data.Instruction>()
                    {
                        new SharpJIT.Loader.Data.Instruction(OpCodes.Return)
                    });

                var pointAddFunction = new SharpJIT.Loader.Data.Function(
                    "add",
                    new List<string>(),
                    intType.Name,
                    "Point",
                    false,
                    new List<string>(),
                    new List<SharpJIT.Loader.Data.Instruction>()
                    {
                        new SharpJIT.Loader.Data.Instruction(OpCodes.LoadArgument, 0),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.LoadField, "Point::x"),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.LoadArgument, 0),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.LoadField, "Point::y"),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.AddInt),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.Return)
                    });

                var mainFunction = new SharpJIT.Loader.Data.Function(
                    "main",
                    new List<string>(),
                    intType.Name,
                    new List<string>() { "Ref.Point" },
                    new List<SharpJIT.Loader.Data.Instruction>()
                    {
                        new SharpJIT.Loader.Data.Instruction(OpCodes.NewObject, ".constructor", "Point", new List<string>()),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.StoreLocal, 0),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.LoadLocal, 0),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.LoadInt, 4711),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.StoreField, "Point::x"),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.LoadLocal, 0),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.LoadInt, 1337),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.StoreField, "Point::y"),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.LoadLocal, 0),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.CallInstance, "add", "Point", new List<string>()),
                        new SharpJIT.Loader.Data.Instruction(OpCodes.Return)
                    });

                var assembly = new SharpJIT.Loader.Data.Assembly(
                    "test",
                    new List<SharpJIT.Loader.Data.Class>() { classDef },
                    new List<SharpJIT.Loader.Data.Function>() { pointConstructor, pointAddFunction, mainFunction });

                container.VirtualMachine.LoadAssembly(assembly);
                var result = container.Execute();
                Assert.AreEqual(1337 + 4711, result);
            }
        }
    }
}
