using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Compiler
{
    /// <summary>
    /// Represents a unresolved branch target
    /// </summary>
    public class UnresolvedBranchTarget
    {
        /// <summary>
        /// Returns the target
        /// </summary>
        public int Target { get; }

        /// <summary>
        /// Returns the size of the branch instruction
        /// </summary>
        public int InstructionSize { get; }

        /// <summary>
        /// Creates a new unresolved branch target
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="instructionSize">The size of the branch instruction</param>
        public UnresolvedBranchTarget(int target, int instructionSize)
        {
            this.Target = target;
            this.InstructionSize = instructionSize;
        }
    }
}
