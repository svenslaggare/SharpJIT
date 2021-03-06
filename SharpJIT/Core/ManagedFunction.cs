﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core
{
	/// <summary>
	/// Represents a managed function
	/// </summary>
	public sealed class ManagedFunction
	{
        /// <summary>
        /// The function definition
        /// </summary>
        public FunctionDefinition Definition { get; }

        /// <summary>
        /// Returns the instructions
        /// </summary>
        public IReadOnlyList<Instruction> Instructions { get; }

        /// <summary>
        /// The type of locals
        /// </summary>
        public IReadOnlyList<BaseType> Locals { get; }

        /// <summary>
        /// The size of the operand stack
        /// </summary>
        public int OperandStackSize { get; set; }

        /// <summary>
        /// The generated code
        /// </summary>
        public IList<byte> GeneratedCode { get; } = new List<byte>();

        /// <summary>
        /// The operand types for instructions
        /// </summary>
        public IReadOnlyList<IList<BaseType>> OperandTypes { get; }

        /// <summary>
        /// Creates a new managed function
        /// </summary>
        /// <param name="definition">The function definition</param>
        /// <param name="locals">The type of the locals</param>
        /// <param name="instructions">The instructions</param>
        public ManagedFunction(FunctionDefinition definition, IList<BaseType> locals, IList<Instruction> instructions)
        {
            this.Definition = definition;
            this.Instructions = new ReadOnlyCollection<Instruction>(new List<Instruction>(instructions));
            this.Locals = new ReadOnlyCollection<BaseType>(new List<BaseType>(locals));

            this.OperandTypes = new ReadOnlyCollection<IList<BaseType>>(
                instructions.Select<Instruction, IList<BaseType>>(x => new List<BaseType>()).ToList());
        }

        public override string ToString()
        {
            return this.Definition.ToString();
        }
    }
}
