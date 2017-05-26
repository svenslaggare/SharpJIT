using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Compiler.Win64
{
	/// <summary>
	/// Represents the Windows API
	/// </summary>
	public static class WinAPI
	{
		/// <summary>
		/// The allocation types
		/// </summary>
		[Flags()]
		public enum AllocationType : uint
		{
			Commit = 0x1000,
			Reserve = 0x2000,
			Reset = 0x80000,
			LargePages = 0x20000000,
			Physical = 0x400000,
			TopDown = 0x100000,
			WriteWatch = 0x200000
		}

		/// <summary>
		/// The memory protection modes
		/// </summary>
		[Flags()]
		public enum MemoryProtection : uint
		{
			Execute = 0x10,
			ExecuteRead = 0x20,
			ExecuteReadWrite = 0x40,
			ExecuteWriteCopy = 0x80,
			NoAccess = 0x01,
			Readonly = 0x02,
			ReadWrite = 0x04,
			WriteCopy = 0x08,
			Guard = 0x100,
			NoCache = 0x200,
			WriteCombine = 0x400
		}

		/// <summary>
		/// The free types
		/// </summary>
		public enum FreeType : uint
		{
			Decommit = 0x4000,
			Release = 0x8000
		}

		/// <summary>
		/// Allocates virtual memory
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr VirtualAlloc(IntPtr address, uint size, AllocationType allocationType, MemoryProtection protect);

		/// <summary>
		/// Frees virtual memory
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool VirtualFree(IntPtr address, uint size, FreeType freeType);

        /// <summary>
        /// Changes the protection mode for the given virtual memory
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtect(IntPtr address, uint size, MemoryProtection newProtect, out MemoryProtection oldProtect);

        /// <summary>
        /// Exits the current process
        /// </summary>
        /// <param name="exitCode">The exit code</param>
        [DllImport("kernel32.dll")]
        public static extern void ExitProcess(uint exitCode);
    }
}
