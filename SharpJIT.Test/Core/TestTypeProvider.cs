using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Core;
using SharpJIT.Core.Objects;

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
            var typeProvider = new TypeProvider(null);

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
            var typeProvider = new TypeProvider(null);

            var intType = typeProvider.FindType("Int");
            var arrayIntType = typeProvider.FindType(TypeSystem.ArrayTypeName(intType));

            Assert.IsNotNull(arrayIntType);
            Assert.AreEqual(arrayIntType.Name, typeProvider.FindType(TypeSystem.ArrayTypeName(intType)).Name);
            Assert.AreEqual(intType, (arrayIntType as ArrayType).ElementType);
        }

        /// <summary>
        /// Tests creating class type
        /// </summary>
        [TestMethod]
        public void TestCreateClassType()
        {
            var classMetadataProvider = new ClassMetadataProvider();
            var typeProvider = new TypeProvider(classMetadataProvider);

            classMetadataProvider.Add(new ClassMetadata("List"));

            var listType = typeProvider.FindType("Ref.List");
            Assert.IsNotNull(listType);
            Assert.AreEqual("Ref.List", listType.Name);
        }

        /// <summary>
        /// Tests creating class type
        /// </summary>
        [TestMethod]
        public void TestCreateClassType2()
        {
            var classMetadataProvider = new ClassMetadataProvider();
            var typeProvider = new TypeProvider(classMetadataProvider);

            var listType = typeProvider.FindClassType("List");
            Assert.IsNull(listType);
        }

        /// <summary>
        /// Tests creating class type
        /// </summary>
        [TestMethod]
        public void TestCreateClassType3()
        {
            var classMetadataProvider = new ClassMetadataProvider();
            var typeProvider = new TypeProvider(classMetadataProvider);

            classMetadataProvider.Add(new ClassMetadata("Point"));

            var pointType = typeProvider.FindClassType("Point");
            var pointArrayType = typeProvider.FindArrayType(pointType);

            Assert.IsNotNull(pointArrayType);
            Assert.AreEqual("Ref.Array[Ref.Point]", pointArrayType.Name);
        }
    }
}
