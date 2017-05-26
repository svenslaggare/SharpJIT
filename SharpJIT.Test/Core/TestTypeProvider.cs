using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Core;

namespace SharpJIT.Test.Core
{
    /// <summary>
    /// Tests the type provider
    /// </summary>
    [TestClass]
    public class TestTypeProvider
    {
        /// <summary>
        /// Tests creating primitive types
        /// </summary>
        [TestMethod]
        public void TestCreatePrimitive()
        {
            var typeProvider = new TypeProvider();

            var intType = typeProvider.FindType("Int");
            Assert.IsNotNull(intType);
            Assert.AreEqual(intType.Name, "Int");

            intType = typeProvider.FindType("Int");
            Assert.IsNotNull(intType);
            Assert.AreEqual(intType.Name, "Int");

            var floatType = typeProvider.FindType("Float");
            Assert.IsNotNull(floatType);
            Assert.AreEqual(floatType.Name, "Float");

            floatType = typeProvider.FindType("Float");
            Assert.IsNotNull(floatType);
            Assert.AreEqual(floatType.Name, "Float");
        }

        /// <summary>
        /// Tests creating array type
        /// </summary>
        [TestMethod]
        public void TestCreateArray()
        {
            var typeProvider = new TypeProvider();

            var intType = typeProvider.FindType("Int");
            var arrayIntType = typeProvider.FindType(TypeSystem.ArrayTypeName(intType));

            Assert.IsNotNull(arrayIntType);
            Assert.AreEqual(arrayIntType.Name, typeProvider.FindType(TypeSystem.ArrayTypeName(intType)).Name);
            Assert.AreEqual(intType, (arrayIntType as ArrayType).ElementType);
        }
    }
}
