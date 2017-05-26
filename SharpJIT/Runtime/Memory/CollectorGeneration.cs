using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;

namespace SharpJIT.Runtime.Memory
{
    /// <summary>
    /// Represents a generation for the garbage collector
    /// </summary>
    public sealed class CollectorGeneration
    {
        private readonly ManagedHeap heap;

        /// <summary>
        /// Creates a new collector generation
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="size">The size of the heap</param>
        public CollectorGeneration(MemoryManager memoryManager, int size)
        {
            this.heap = new ManagedHeap(memoryManager, size);
        }

        /// <summary>
        /// Allocates an object of the given size
        /// </summary>
        /// <param name="size">The size of the object</param>
        public IntPtr Allocate(int size)
        {
            var objectPointer = this.heap.Allocate(size);
            if (objectPointer != IntPtr.Zero)
            {

            }
            else
            {
                throw new ArgumentException("Could not allocate object.");
            }

            return objectPointer;
        } 
    }
}
