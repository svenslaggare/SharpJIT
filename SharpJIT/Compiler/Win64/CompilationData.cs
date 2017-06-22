using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAssembler.x64;
using SharpJIT.Core;
using SharpJIT.Runtime;

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
        /// The assembler
        /// </summary>
        public Assembler Assembler { get; }

        /// <summary>
        /// The operand stack
        /// </summary>
        public OperandStack OperandStack { get; }

        /// <summary>
        /// Creates new compilation data
        /// </summary>
        /// <param name="function">The function</param>
        public CompilationData(ManagedFunction function)
            : base(function)
        {
            this.Assembler = new Assembler(function.GeneratedCode);
            this.OperandStack = new OperandStack(function, this.Assembler);
        }
    }
}
