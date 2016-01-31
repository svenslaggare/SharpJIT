using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Compiler
{
    /// <summary>
    /// Manages read-only memory
    /// </summary>
    public class ReadonlyMemory
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
        public void MakeReadonly()
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
    public class CodeMemory
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
    public class MemoryManager : IDisposable
    {
        private readonly IList<MemoryPage> pages = new List<MemoryPage>();
        private readonly CodeMemory codeMemory = new CodeMemory();
        private readonly ReadonlyMemory readonlyMemory = new ReadonlyMemory();

        private readonly int pageSize = 4096;

        /// <summary>
        /// Creates a new page
        /// </summary>
        /// <param name="minSize">The minimum size required by the page</param>
        private MemoryPage CreatePage(int minSize)
        {
            //Align the size of the page to page sizes.
            int size = (minSize + (this.pageSize - 1) / this.pageSize) * this.pageSize;

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
        public IntPtr AllocateReadonly(float value)
        {
            if (!this.readonlyMemory.Values.ContainsKey(value))
            {
                IntPtr valuePtr = IntPtr.Zero;
                int size = sizeof(float);

                if (this.readonlyMemory.ActivePage == null)
                {
                    this.readonlyMemory.AddPage(this.CreatePage(size));
                    valuePtr = this.readonlyMemory.ActivePage.Allocate(size).Value;
                }
                else
                {
                    var memory = this.readonlyMemory.ActivePage.Allocate(size);

                    //Check if active page has any room
                    if (memory != null)
                    {
                        valuePtr = memory.Value;
                    }
                    else
                    {
                        this.readonlyMemory.AddPage(this.CreatePage(size));
                        valuePtr = this.readonlyMemory.ActivePage.Allocate(size).Value;
                    }
                }

                NativeHelpers.CopyTo(valuePtr, BitConverter.GetBytes(value));
                this.readonlyMemory.Values.Add(value, valuePtr);
                return valuePtr;
            }
            else
            {
                return this.readonlyMemory.Values[value];
            }
        }

        /// <summary>
        /// Makes the allocated memory executable (and not writable)
        /// </summary>
        public void MakeExecutable()
        {
            this.readonlyMemory.MakeReadonly();
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
