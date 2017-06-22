using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core;

namespace SharpJIT.Compiler
{
    /// <summary>
    /// Represents a unresolved branch target
    /// </summary>
    public sealed class UnresolvedBranchTarget
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

    /// <summary>
    /// The function call address modes
    /// </summary>
    public enum FunctionCallAddressMode
    {
        Absolute,
        Relative
    }

    /// <summary>
    /// Represents a unresolved function call
    /// </summary>
    public sealed class UnresolvedFunctionCall
    {
        /// <summary>
        /// The address mode
        /// </summary>
        public FunctionCallAddressMode AddressMode { get; }

        /// <summary>
        /// The function to call
        /// </summary>
        public FunctionDefinition Function { get; }

        /// <summary>
        /// The offset in the function that makes the call
        /// </summary>
        public int CallSiteOffset { get; }

        /// <summary>
        /// Creates a new unresolved call
        /// </summary>
        /// <param name="addressMode">The address mode</param>
        /// <param name="function">The function</param>
        /// <param name="callSiteOffset">The offset in the function that makes the call</param>
        public UnresolvedFunctionCall(FunctionCallAddressMode addressMode, FunctionDefinition function, int callSiteOffset)
        {
            this.AddressMode = addressMode;
            this.Function = function;
            this.CallSiteOffset = callSiteOffset;
        }
    }

    /// <summary>
    /// Holds compilation data
    /// </summary>
    public abstract class AbstractCompilationData
    {
        /// <summary>
        /// The function
        /// </summary>
        public ManagedFunction Function { get; }

        /// <summary>
        /// Returns the definition of the function to compile
        /// </summary>
        public FunctionDefinition FunctionDefinition => this.Function.Definition;

        /// <summary>
        /// Mapping from instruction number to native instruction offset
        /// </summary>
        public IList<int> InstructionMapping { get; } = new List<int>();

        /// <summary>
        /// The unresolved function calls
        /// </summary>
        public IList<UnresolvedFunctionCall> UnresolvedFunctionCalls { get; } = new List<UnresolvedFunctionCall>();

        /// <summary>
        /// The unresolved branches
        /// </summary>
        public IDictionary<int, UnresolvedBranchTarget> UnresolvedBranches { get; } = new Dictionary<int, UnresolvedBranchTarget>();

        /// <summary>
        /// The unresolved native labels
        /// </summary>
        public IDictionary<int, IntPtr> UnresolvedNativeLabels { get; } = new Dictionary<int, IntPtr>();

        /// <summary>
        /// Creates new compilation data
        /// </summary>
        /// <param name="function">The function</param>
        public AbstractCompilationData(ManagedFunction function)
        {
            this.Function = function;
        }
    }
}
