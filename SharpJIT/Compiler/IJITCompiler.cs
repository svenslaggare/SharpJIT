using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core;

namespace SharpJIT.Compiler
{
    /// <summary>
    /// Represents an entry point
    /// </summary>
    public delegate int EntryPoint();

    /// <summary>
    /// Represents an interface for a JIT compiler
    /// </summary>
    public interface IJITCompiler
    {
        /// <summary>
        /// Returns the compilation data for the given function
        /// </summary>
        /// <param name="function">The function</param>
        /// <returns>The data or null if not compiled</returns>
        AbstractCompilationData GetCompilationData(ManagedFunction function);

        /// <summary>
        /// Compiles the given function
        /// </summary>
        /// <param name="function">The function to compile</param>
        /// <returns>A pointer to the start of the compiled function</returns>
        IntPtr Compile(ManagedFunction function);

        /// <summary>
        /// Makes the compiled functions executable
        /// </summary>
        void MakeExecutable();
    }
}
