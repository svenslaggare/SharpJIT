using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core;

namespace SharpJIT.Compiler.Win64
{
    /// <summary>
    /// Holds compilation data
    /// </summary>
    public class CompilationData : AbstractCompilationData
    {
        /// <summary>
        /// The size of the stack
        /// </summary>
        public int StackSize { get; set; }

        /// <summary>
        /// The operand stack
        /// </summary>
        /// <remarks>Only has value if the function is not optimized.</remarks>
        public OperandStack OperandStack { get; }

        /// <summary>
        /// Creates new compilation data
        /// </summary>
        /// <param name="virtualMachine">The virtual macine</param>
        /// <param name="function">The function</param>
        public CompilationData(VirtualMachine virtualMachine, Function function)
            : base(function)
        {
            this.OperandStack = new OperandStack(function);
        }
    }
}
