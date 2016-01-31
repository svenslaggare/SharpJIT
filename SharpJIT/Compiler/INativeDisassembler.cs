using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Compiler
{
    /// <summary>
    /// Represents a native disassembler
    /// </summary>
    public interface INativeDisassembler
    {
        /// <summary>
        /// Disassembles the code block starting at the given index
        /// </summary>
        /// <param name="index">The start of the block</param>
        /// <param name="size">The size of the block</param>
        /// <param name="output">The output</param>
        void DisassembleBlock(int index, int size, StringBuilder output);
    }
}
