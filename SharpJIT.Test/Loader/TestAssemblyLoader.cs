using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Core;
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
            using (var virtualMachine = new VirtualMachine(vm => null))
            {
                var intType = virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
                var floatType = virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);

                var functionLoader = new FunctionLoader(virtualMachine);
                var parsedFunction = new SharpJIT.Loader.Parser.Function(
                    "test",
                    new List<string>() { intType.Name },
                    intType.Name,
                    new List<string>() { floatType.Name },
                    new List<SharpJIT.Loader.Parser.Instruction>()
                    {
                        new SharpJIT.Loader.Parser.Instruction(OpCodes.LoadInt, 1337),
                        new SharpJIT.Loader.Parser.Instruction(OpCodes.Return)
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
        }

        /// <summary>
        /// Tests loading a class
        /// </summary>
        [TestMethod]
        public void TestClassLoader()
        {
            using (var virtualMachine = new VirtualMachine(vm => null))
            {
                var intType = virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

                var classLoader = new ClassLoader(virtualMachine);

                var classDef = new SharpJIT.Loader.Parser.Class(
                    "Point",
                    new List<SharpJIT.Loader.Parser.Field>()
                    {
                        new SharpJIT.Loader.Parser.Field("x", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public),
                        new SharpJIT.Loader.Parser.Field("y", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public)
                    });

                classLoader.LoadClasses(new List<SharpJIT.Loader.Parser.Class>() { classDef });

                var classMetadata = virtualMachine.ClassMetadataProvider.GetMetadata(classDef.Name);

                Assert.IsNotNull(classMetadata);
                Assert.AreEqual(classDef.Name, classMetadata.Name);

                var fields = classMetadata.Fields.ToList();
                Assert.AreEqual(2, fields.Count);
                Assert.AreEqual("x", fields[0].Name);
                Assert.AreEqual(intType, fields[0].Type);

                Assert.AreEqual("y", fields[1].Name);
                Assert.AreEqual(intType, fields[1].Type);
            }
        }

        /// <summary>
        /// Tests loading a class
        /// </summary>
        [TestMethod]
        public void TestClassLoader2()
        {
            using (var virtualMachine = new VirtualMachine(vm => null))
            {
                var intType = virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);

                var classLoader = new ClassLoader(virtualMachine);

                var listClassDef = new SharpJIT.Loader.Parser.Class(
                    "List",
                    new List<SharpJIT.Loader.Parser.Field>()
                    {
                        new SharpJIT.Loader.Parser.Field("head", "Ref.Point", SharpJIT.Core.Objects.AccessModifier.Public)
                    });

                var pointClassDef = new SharpJIT.Loader.Parser.Class(
                    "Point",
                    new List<SharpJIT.Loader.Parser.Field>()
                    {
                        new SharpJIT.Loader.Parser.Field("x", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public),
                        new SharpJIT.Loader.Parser.Field("y", intType.Name, SharpJIT.Core.Objects.AccessModifier.Public)
                    });

                classLoader.LoadClasses(new List<SharpJIT.Loader.Parser.Class>() { listClassDef, pointClassDef });

                Assert.IsNotNull(virtualMachine.ClassMetadataProvider.GetMetadata(pointClassDef.Name));

                var listMetadata = virtualMachine.ClassMetadataProvider.GetMetadata(listClassDef.Name);
                Assert.IsNotNull(listMetadata);

                var pointType = virtualMachine.TypeProvider.FindType("Ref.Point", false);
                Assert.IsNotNull(pointType);

                Assert.AreEqual(pointType, listMetadata.Fields.First().Type);
            }
        }
    }
}
