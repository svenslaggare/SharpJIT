using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core;

namespace SharpJIT.Compiler
{
    /// <summary>
    /// The function call address modes
    /// </summary>
    public enum FunctionCallAddressModes
    {
        Absolute,
        Relative
    }

    /// <summary>
    /// Represents a unresolved function call
    /// </summary>
    public class UnresolvedFunctionCall
    {
        /// <summary>
        /// The address mode
        /// </summary>
        public FunctionCallAddressModes AddressMode { get; }

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
        public UnresolvedFunctionCall(FunctionCallAddressModes addressMode, FunctionDefinition function, int callSiteOffset)
        {
            this.AddressMode = addressMode;
            this.Function = function;
            this.CallSiteOffset = callSiteOffset;
        }
    }
}
