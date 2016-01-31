using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Compiler
{
    /// <summary>
    /// Represents a memory page
    /// </summary>
    public class MemoryPage : IDisposable
    {
        private readonly IntPtr start;
        private readonly int size;
        private int used;

        /// <summary>
        /// Creates a new memory page
        /// </summary>
        /// <param name="start">The start of the code page</param>
        /// <param name="size">The size</param>
        public MemoryPage(IntPtr start, int size)
        {
            this.start = start;
            this.size = size;
            this.used = 0;
        }

        /// <summary>
        /// Allocates memory of the given size
        /// </summary>
        /// <param name="size">The size of the allocation</param>
        /// <returns>The start of the allocation or null if there is not enough room.</returns>
        public IntPtr? Allocate(int size)
        {
            if (this.used + size < this.size)
            {
                var newPtr = IntPtr.Add(this.start, this.used);
                this.used += size;
                return newPtr;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the protection mode
        /// </summary>
        /// <param name="mode">The protection mode</param>
        public void SetProtectionMode(WinAPI.MemoryProtection mode)
        {
            WinAPI.MemoryProtection old;
            WinAPI.VirtualProtect(this.start, (uint)this.size, mode, out old);
        }

        /// <summary>
        /// Disposes the underlying memory
        /// </summary>
        public void Dispose()
        {
            WinAPI.VirtualFree(this.start, (uint)this.size, WinAPI.FreeType.Release);
        }
    }
}
