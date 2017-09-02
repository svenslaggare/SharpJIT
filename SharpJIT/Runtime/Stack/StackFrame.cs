using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Core;

namespace SharpJIT.Runtime.Stack
{
    /// <summary>
    /// Represents an entry for the stack frame (argument, local, operand)
    /// </summary>
    public struct StackFrameEntry
    {
        /// <summary>
        /// A pointer to the value for the entry
        /// </summary>
        public IntPtr ValuePointer { get; }

        /// <summary>
        /// The type of the entry
        /// </summary>
        public BaseType Type { get; }

        /// <summary>
        /// Creates a new stack frame entry
        /// </summary>
        /// <param name="valuePointer">The pointer to the value</param>
        /// <param name="type">The type of the value</param>
        public StackFrameEntry(IntPtr valuePointer, BaseType type)
        {
            this.ValuePointer = valuePointer;
            this.Type = type;
        }

        /// <summary>
        /// Returns the value of the entry
        /// </summary>
        public long Value => NativeHelpers.ReadLong(this.ValuePointer);
    }

    /// <summary>
    /// Represents a stack frame
    /// </summary>
    public struct StackFrame
    {
        /// <summary>
        /// The base pointer for the frame
        /// </summary>
        public IntPtr BasePointer { get; }

        /// <summary>
        /// The function for the frame
        /// </summary>
        public ManagedFunction Function { get; }

        /// <summary>
        /// The index of the current instruction in the frame
        /// </summary>
        public int InstructionIndex { get; }

        /// <summary>
        /// Creates a new stack frame for the given function
        /// </summary>
        /// <param name="basePointer">The base pointer</param>
        /// <param name="function">The function</param>
        /// <param name="instructionIndex">The index of the current instruction</param>
        public StackFrame(IntPtr basePointer, ManagedFunction function, int instructionIndex)
        {
            this.BasePointer = basePointer;
            this.Function = function;
            this.InstructionIndex = instructionIndex;
        }

        /// <summary>
        /// Creates a new stack frame from the given entry of the call stack
        /// </summary>
        /// <param name="callStackEntry">The entry on the call stack</param>
        public StackFrame(CallStackEntry callStackEntry)
            : this(callStackEntry.BasePointer, callStackEntry.Function, callStackEntry.InstructionIndex)
        {

        }

        /// <summary>
        /// Returns the types for the operands to the current instruction
        /// </summary>
        private IList<BaseType> OperandTypes => this.Function.OperandTypes[this.InstructionIndex];

        /// <summary>
        /// Returns the size of the operand stack
        /// </summary>
        public int OperandStackSize => this.OperandTypes.Count;

        /// <summary>
        /// Returns the frame entry for the given function argument
        /// </summary>
        /// <param name="index">The index of the argument</param>
        public StackFrameEntry GetArgument(int index)
        {
            return new StackFrameEntry(
                NativeHelpers.AddOffsetToIntPointer(this.BasePointer, -8 * (1 + index)),
                this.Function.Definition.Parameters[index]);
        }

        /// <summary>
        /// Returns the function arguments
        /// </summary>
        public IEnumerable<StackFrameEntry> GetArguments()
        {
            for (int i = 0; i < this.Function.Definition.Parameters.Count; i++)
            {
                yield return this.GetArgument(i);
            }
        }

        /// <summary>
        /// Returns the frame entry for the given local
        /// </summary>
        /// <param name="index">The index of the local</param>
        public StackFrameEntry GetLocal(int index)
        {
            return new StackFrameEntry(
                NativeHelpers.AddOffsetToIntPointer(this.BasePointer, -8 * (1 + this.Function.Definition.Parameters.Count + index)),
                this.Function.Locals[index]);
        }

        /// <summary>
        /// Returns the locals
        /// </summary>
        public IEnumerable<StackFrameEntry> GetLocals()
        {
            for (int i = 0; i < this.Function.Locals.Count; i++)
            {
                yield return this.GetLocal(i);
            }
        }

        /// <summary>
        /// Returns the frame entry for the given operand for the current instruction
        /// </summary>
        /// <param name="index">The index of the operand</param>
        public StackFrameEntry GetStackOperand(int index)
        {
            return new StackFrameEntry(
                NativeHelpers.AddOffsetToIntPointer(
                    this.BasePointer,
                    -8 * (1 + this.Function.Definition.Parameters.Count + this.Function.Locals.Count + index)),
                this.OperandTypes[this.OperandTypes.Count - 1 - index]);
        }

        /// <summary>
        /// Returns the stack operands
        /// </summary>
        public IEnumerable<StackFrameEntry> GetStackOperands()
        {
            for (int i = 0; i < this.OperandStackSize; i++)
            {
                yield return this.GetStackOperand(i);
            }
        }
    }
}
