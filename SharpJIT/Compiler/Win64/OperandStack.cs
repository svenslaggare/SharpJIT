using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAssembler;
using SharpAssembler.x64;
using SharpJIT.Core;

namespace SharpJIT.Compiler.Win64
{
    /// <summary>
    /// Represents an operand stack
    /// </summary>
    public class OperandStack
    {
        private readonly Function function;
        private int operandTopIndex;

        /// <summary>
        /// Creates a new operand stack
        /// </summary>
        /// <param name="function">The function</param>
        public OperandStack(Function function)
        {
            this.function = function;
            this.operandTopIndex = -1;
        }

        /// <summary>
        /// Returns the number of operands on the stack
        /// </summary>
        public int NumStackOperands
        {
            get { return this.operandTopIndex + 1; }
        }

        /// <summary>
        /// Asserts that the operand stack is not empty
        /// </summary>
        private void AssertNotEmpty()
        {
            if (this.operandTopIndex <= -1)
            {
                throw new InvalidOperationException("The operand stack is empty.");
            }
        }

        /// <summary>
        /// Calculates the offset in the stack frame for the given stack operand
        /// </summary>
        /// <param name="operandStackIndex">The index of the stack operand</param>
        private int GetStackOperandOffset(int operandStackIndex)
        {
            return 
                -Assembler.RegisterSize
                * (1 + this.function.Locals.Count + this.function.Definition.Parameters.Count + operandStackIndex);
        }

        /// <summary>
        /// Pops an operand from the operand stack to the given register
        /// </summary>
        /// <param name="register">The register to pop to</param>
        public void PopRegister(IntRegister register)
        {
            this.AssertNotEmpty();

            int stackOffset = GetStackOperandOffset(this.operandTopIndex);
            Assembler.Move(
                this.function.GeneratedCode,
                register,
                new MemoryOperand(Register.BP, stackOffset)); //mov <reg>, [rbp+<operand offset>]

            this.operandTopIndex--;
        }

        /// <summary>
        /// Pops an operand from the operand stack to the given register
        /// </summary>
        /// <param name="register">The register</param>
        public void PopRegister(FloatRegister register)
        {
            this.AssertNotEmpty();

            int stackOffset = GetStackOperandOffset(this.operandTopIndex);
            Assembler.Move(
                this.function.GeneratedCode,
                register,
                new MemoryOperand(Register.BP, stackOffset));

            this.operandTopIndex--;
        }

        /// <summary>
        /// Pushes the given register to the operand stack
        /// </summary>
        /// <param name="register">The register</param>
        public void PushRegister(IntRegister register)
        {
            this.operandTopIndex++;
            int stackOffset = GetStackOperandOffset(this.operandTopIndex);

            //mov [rbp+<operand offset>], <reg>
            Assembler.Move(
                this.function.GeneratedCode,
                new MemoryOperand(Register.BP, stackOffset),
                register);
        }

        /// <summary>
        /// Pushes the given register to the operand stack
        /// </summary>
        /// <param name="register">The register</param>
        public void PushRegister(FloatRegister register)
        {
            this.operandTopIndex++;
            int stackOffset = GetStackOperandOffset(this.operandTopIndex);

            //movss [rbp+<operand offset>], <reg>
            Assembler.Move(
                this.function.GeneratedCode,
                new MemoryOperand(Register.BP, stackOffset),
                register);
        }

        /// <summary>
        /// Pushes the given value to the operand stack
        /// </summary>
        /// <param name="value">The value</param>
        public void PushInt(int value)
        {
            this.operandTopIndex++;
            int stackOffset = GetStackOperandOffset(this.operandTopIndex);

            //mov [rbp+<operand offset>], value
            Assembler.Move(
                this.function.GeneratedCode,
                new MemoryOperand(Register.BP, stackOffset),
                value);
        }
    }
}
