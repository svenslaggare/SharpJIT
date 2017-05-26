using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;

namespace SharpJIT.Runtime.Memory
{
    /// <summary>
    /// Represents a managed heap
    /// </summary>
    public sealed class ManagedHeap
    {
        private readonly MemoryPage dataPage;
        private IntPtr nextAllocation;

        /// <summary>
        /// Creates a new heap of the given size
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="size">The size of the heap</param>
        public ManagedHeap(MemoryManager memoryManager, int size)
        {
            this.dataPage = memoryManager.CreatePage(size);
            this.nextAllocation = this.dataPage.Start;
        }

        /// <summary>
        /// Returns the start of the heap
        /// </summary>
        public IntPtr Start
        {
            get { return this.dataPage.Start; }
        }

        /// <summary>
        /// Returns the end of the heap
        /// </summary>
        public IntPtr End
        {
            get { return this.dataPage.Start + this.dataPage.Size - 1; }
        }

        /// <summary>
        /// Allocates a memory block of the given size.
        /// </summary>
        /// <param name="size">The size of the block</param>
        /// <returns>The memory block or null if not allocated</returns>
        public IntPtr Allocate(int size)
        {
            var nextAllocation = this.nextAllocation + size;

            if (nextAllocation.ToInt64() <= this.End.ToInt64())
            {
                var allocation = this.nextAllocation;
                this.nextAllocation = nextAllocation;
                return allocation;
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Sets where the next allocation should occur.
        /// </summary>
        /// <param name="nextAllocation">The position of the next allocation</param>
        public void SetNextAllocation(IntPtr nextAllocation)
        {
            if (nextAllocation.ToInt64() >= this.Start.ToInt64() && nextAllocation.ToInt64() <= this.End.ToInt64())
            {
                this.nextAllocation = nextAllocation;
            }
            else
            {
                throw new ArgumentException("The pointer is outside of the heap.");
            }
        }
    }
}
