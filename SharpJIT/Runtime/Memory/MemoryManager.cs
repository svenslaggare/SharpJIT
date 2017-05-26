using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Compiler.Win64;

namespace SharpJIT.Runtime.Memory
{
    /// <summary>
    /// Represents a memory page
    /// </summary>
    public sealed class MemoryPage : IDisposable
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
        /// Returns the start of the memory page
        /// </summary>
        public IntPtr Start
        {
            get { return this.start; }
        }

        /// <summary>
        /// Returns the size of the heap
        /// </summary>
        public int Size
        {
            get { return this.size; }
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
            WinAPI.VirtualProtect(this.start, (uint)this.size, mode, out var old);
        }

        /// <summary>
        /// Disposes the underlying memory
        /// </summary>
        public void Dispose()
        {
            WinAPI.VirtualFree(this.start, (uint)this.size, WinAPI.FreeType.Release);
        }
    }

    /// <summary>
    /// Manages read-only memory
    /// </summary>
    public sealed class ReadOnlyMemory
    {
        private readonly IList<MemoryPage> pages = new List<MemoryPage>();

        /// <summary>
        /// The defined values
        /// </summary>
        public IDictionary<ValueType, IntPtr> Values { get; } = new Dictionary<ValueType, IntPtr>();

        /// <summary>
        /// The active page
        /// </summary>
        public MemoryPage ActivePage { get; private set; }

        /// <summary>
        /// Adds the given page to the list of pages and makes it the active one
        /// </summary>
        /// <param name="page">The page</param>
        public void AddPage(MemoryPage page)
        {
            this.pages.Add(page);
            this.ActivePage = page;
        }

        /// <summary>
        /// Makes the allocated memory read-only
        /// </summary>
        public void MakeReadOnly()
        {
            foreach (var page in this.pages)
            {
                page.SetProtectionMode(WinAPI.MemoryProtection.Readonly);
            }
        }
    }

    /// <summary>
    /// Manages code memory
    /// </summary>
    public sealed class CodeMemory
    {
        private readonly IList<MemoryPage> pages = new List<MemoryPage>();

        /// <summary>
        /// The active page
        /// </summary>
        public MemoryPage ActivePage { get; private set; }

        /// <summary>
        /// Adds the given page to the list of pages and makes it the active one
        /// </summary>
        /// <param name="page">The page</param>
        public void AddPage(MemoryPage page)
        {
            this.pages.Add(page);
            this.ActivePage = page;
        }

        /// <summary>
        /// Makes the allocated memory executable (and not writable)
        /// </summary>
        public void MakeExecutable()
        {
            foreach (var page in this.pages)
            {
                page.SetProtectionMode(WinAPI.MemoryProtection.ExecuteRead);
            }
        }
    }

    /// <summary>
    /// Represents a memory manager
    /// </summary>
    public sealed class MemoryManager : IDisposable
    {
        private readonly IList<MemoryPage> pages = new List<MemoryPage>();
        private readonly CodeMemory codeMemory = new CodeMemory();
        private readonly ReadOnlyMemory readOnlyMemory = new ReadOnlyMemory();

        private readonly int pageSize = 4096;

        /// <summary>
        /// Creates a new memory page
        /// </summary>
        /// <param name="minSize">The minimum size required by the page</param>
        public MemoryPage CreatePage(int minSize)
        {
            //Align the size of the page to page sizes.
            int size = ((minSize + this.pageSize - 1) / this.pageSize) * this.pageSize;

            //Allocate writable & readable memory
            var memory = WinAPI.VirtualAlloc(
                IntPtr.Zero,
                (uint)size,
                WinAPI.AllocationType.Commit,
                WinAPI.MemoryProtection.ReadWrite);

            var page = new MemoryPage(memory, size);
            this.pages.Add(page);
            return page;
        }

        /// <summary>
        /// Allocates code memory of the given size
        /// </summary>
        /// <param name="size">The amount to allocate</param>
        /// <returns>Pointer to the allocated memory</returns>
        public IntPtr AllocateCode(int size)
        {
            if (this.codeMemory.ActivePage == null)
            {
                this.codeMemory.AddPage(this.CreatePage(size));
                return this.codeMemory.ActivePage.Allocate(size).Value;
            }
            else
            {
                var memory = this.codeMemory.ActivePage.Allocate(size);

                //Check if active page has any room
                if (memory != null)
                {
                    return memory.Value;
                }
                else
                {
                    this.codeMemory.AddPage(this.CreatePage(size));
                    return this.codeMemory.ActivePage.Allocate(size).Value;
                }
            }
        }
        
        /// <summary>
        /// Allocates read-only memory for the given value
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>Pointer to the allocated memory</returns>
        public IntPtr AllocateReadOnly(float value)
        {
            if (!this.readOnlyMemory.Values.ContainsKey(value))
            {
                IntPtr valuePtr = IntPtr.Zero;
                int size = sizeof(float);

                if (this.readOnlyMemory.ActivePage == null)
                {
                    this.readOnlyMemory.AddPage(this.CreatePage(size));
                    valuePtr = this.readOnlyMemory.ActivePage.Allocate(size).Value;
                }
                else
                {
                    var memory = this.readOnlyMemory.ActivePage.Allocate(size);

                    //Check if active page has any room
                    if (memory != null)
                    {
                        valuePtr = memory.Value;
                    }
                    else
                    {
                        this.readOnlyMemory.AddPage(this.CreatePage(size));
                        valuePtr = this.readOnlyMemory.ActivePage.Allocate(size).Value;
                    }
                }

                NativeHelpers.CopyTo(valuePtr, BitConverter.GetBytes(value));
                this.readOnlyMemory.Values.Add(value, valuePtr);
                return valuePtr;
            }
            else
            {
                return this.readOnlyMemory.Values[value];
            }
        }

        /// <summary>
        /// Makes the allocated memory executable (and not writable)
        /// </summary>
        public void MakeExecutable()
        {
            this.readOnlyMemory.MakeReadOnly();
            this.codeMemory.MakeExecutable();
        }

        /// <summary>
        /// Disposes the pages
        /// </summary>
        public void Dispose()
        {
            foreach (var page in this.pages)
            {
                page.Dispose();
            }
        }
    }
}
