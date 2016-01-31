using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core
{
	/// <summary>
	/// Represents a function
	/// </summary>
	public class Function
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
        /// The types of locals
        /// </summary>
        public IReadOnlyList<VMType> Locals { get; }

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
        public IReadOnlyList<IList<VMType>> OperandTypes { get; }

        /// <summary>
        /// Creates a new function
        /// </summary>
        /// <param name="definition">The function definition</param>
        /// <param name="instructions">The instructions</param>
        /// <param name="locals">The type of the locals</param>
        public Function(FunctionDefinition definition, IList<Instruction> instructions, IList<VMType> locals)
		{
            this.Definition = definition;
            this.Instructions = new ReadOnlyCollection<Instruction>(instructions);
            this.Locals = new ReadOnlyCollection<VMType>(locals);

            this.OperandTypes = new ReadOnlyCollection<IList<VMType>>(
                instructions.Select<Instruction, IList<VMType>>(x => new List<VMType>()).ToList());
        }

        public override string ToString()
        {
            return this.Definition.ToString();
        }
    }
}
