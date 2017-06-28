using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Compiler;
using SharpJIT.Runtime.Memory;

namespace SharpJIT.Test.GC
{
    /// <summary>
    /// Test allocating objects
    /// </summary>
    [TestClass]
    public class TestAllocate
    {
        /// <summary>
        /// Tests allocating objects
        /// </summary>
        [TestMethod]
        public void TestAllocateObject()
        {
            using (var memoryManager = new MemoryManager())
            {
                var generation = new CollectorGeneration(memoryManager, new ManagedObjectReferences(), 1024);
                var objectPointer = generation.Allocate(20);
                Assert.AreNotEqual(IntPtr.Zero, objectPointer);
            }
        }
    }
}
