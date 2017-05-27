using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler.Win64;

namespace SharpJIT.Core
{
    /// <summary>
    /// Represents a pass for instructions
    /// </summary>
    /// <typeparam name="T">Type of extra argument</typeparam>
    public abstract class InstructionPass<T>
    {
        /// <summary>
        /// Handles the given instruction
        /// </summary>
        /// <param name="data">The complation data for the function</param>
        /// <param name="instruction">The instruction</param>
        /// <param name="index">The index of the instruction</param>
        public virtual void Handle(T data, Instruction instruction, int index)
        {
            switch (instruction.OpCode)
            {
                case OpCodes.Pop:
                    this.HandlePop(data, instruction, index);
                    break;
                case OpCodes.LoadInt:
                    this.HandleLoadInt(data, instruction, index);
                    break;
                case OpCodes.LoadFloat:
                    this.HandleLoadFloat(data, instruction, index);
                    break;
                case OpCodes.AddInt:
                    this.HandleAddInt(data, instruction, index);
                    break;
                case OpCodes.SubInt:
                    this.HandleSubInt(data, instruction, index);
                    break;
                case OpCodes.MulInt:
                    this.HandleMulInt(data, instruction, index);
                    break;
                case OpCodes.DivInt:
                    this.HandleDivInt(data, instruction, index);
                    break;
                case OpCodes.AddFloat:
                    this.HandleAddFloat(data, instruction, index);
                    break;
                case OpCodes.SubFloat:
                    this.HandleSubFloat(data, instruction, index);
                    break;
                case OpCodes.MulFloat:
                    this.HandleMulFloat(data, instruction, index);
                    break;
                case OpCodes.DivFloat:
                    this.HandleDivFloat(data, instruction, index);
                    break;
                case OpCodes.LoadTrue:
                    this.HandleLoadTrue(data, instruction, index);
                    break;
                case OpCodes.LoadFalse:
                    this.HandleLoadFalse(data, instruction, index);
                    break;
                case OpCodes.And:
                    this.HandleAnd(data, instruction, index);
                    break;
                case OpCodes.Or:
                    this.HandleOr(data, instruction, index);
                    break;
                case OpCodes.Not:
                    this.HandleNot(data, instruction, index);
                    break;
                case OpCodes.Call:
                    this.HandleCall(data, instruction, index);
                    break;
                case OpCodes.Return:
                    this.HandleReturn(data, instruction, index);
                    break;
                case OpCodes.LoadArgument:
                    this.HandleLoadArgument(data, instruction, index);
                    break;
                case OpCodes.LoadLocal:
                    this.HandleLoadLocal(data, instruction, index);
                    break;
                case OpCodes.StoreLocal:
                    this.HandleStoreLocal(data, instruction, index);
                    break;
                case OpCodes.Branch:
                    this.HandleBranch(data, instruction, index);
                    break;
                case OpCodes.BranchEqual:
                    this.HandleBranchEqual(data, instruction, index);
                    break;
                case OpCodes.BranchNotEqual:
                    this.HandleBranchNotEqual(data, instruction, index);
                    break;
                case OpCodes.BranchGreaterThan:
                    this.HandleBranchGreaterThan(data, instruction, index);
                    break;
                case OpCodes.BranchGreaterThanOrEqual:
                    this.HandleBranchGreaterThanOrEqual(data, instruction, index);
                    break;
                case OpCodes.BranchLessThan:
                    this.HandleBranchLessThan(data, instruction, index);
                    break;
                case OpCodes.BranchLessOrEqual:
                    this.HandleBranchLessThanOrEqual(data, instruction, index);
                    break;
                case OpCodes.CompareEqual:
                    this.HandleCompareEqual(data, instruction, index);
                    break;
                case OpCodes.CompareNotEqual:
                    this.HandleCompareNotEqual(data, instruction, index);
                    break;
                case OpCodes.CompareGreaterThan:
                    this.HandleCompareGreaterThan(data, instruction, index);
                    break;
                case OpCodes.CompareGreaterThanOrEqual:
                    this.HandleCompareGreaterThanOrEqual(data, instruction, index);
                    break;
                case OpCodes.CompareLessThan:
                    this.HandleCompareLessThan(data, instruction, index);
                    break;
                case OpCodes.CompareLessThanOrEqual:
                    this.HandleCompareLessThanOrEqual(data, instruction, index);
                    break;
                case OpCodes.LoadNull:
                    this.HandleLoadNull(data, instruction, index);
                    break;
                case OpCodes.NewArray:
                    this.HandleNewArray(data, instruction, index);
                    break;
                case OpCodes.LoadArrayLength:
                    this.HandleLoadArrayLength(data, instruction, index);
                    break;
                case OpCodes.StoreElement:
                    this.HandleStoreElement(data, instruction, index);
                    break;
                case OpCodes.LoadElement:
                    this.HandleLoadElement(data, instruction, index);
                    break;
            }
        }

        protected abstract void HandlePop(T data, Instruction instruction, int index);

        protected abstract void HandleLoadInt(T data, Instruction instruction, int index);
        protected abstract void HandleLoadFloat(T data, Instruction instruction, int index);

        protected abstract void HandleAddInt(T data, Instruction instruction, int index);
        protected abstract void HandleSubInt(T data, Instruction instruction, int index);
        protected abstract void HandleMulInt(T data, Instruction instruction, int index);
        protected abstract void HandleDivInt(T data, Instruction instruction, int index);

        protected abstract void HandleAddFloat(T data, Instruction instruction, int index);
        protected abstract void HandleSubFloat(T data, Instruction instruction, int index);
        protected abstract void HandleMulFloat(T data, Instruction instruction, int index);
        protected abstract void HandleDivFloat(T data, Instruction instruction, int index);

        protected abstract void HandleLoadTrue(T data, Instruction instruction, int index);
        protected abstract void HandleLoadFalse(T data, Instruction instruction, int index);

        protected abstract void HandleAnd(T data, Instruction instruction, int index);
        protected abstract void HandleOr(T data, Instruction instruction, int index);
        protected abstract void HandleNot(T data, Instruction instruction, int index);

        protected abstract void HandleLoadLocal(T data, Instruction instruction, int index);
        protected abstract void HandleStoreLocal(T data, Instruction instruction, int index);

        protected abstract void HandleCall(T data, Instruction instruction, int index);
        protected abstract void HandleReturn(T data, Instruction instruction, int index);
        protected abstract void HandleLoadArgument(T data, Instruction instruction, int index);

        protected abstract void HandleBranch(T data, Instruction instruction, int index);
        protected abstract void HandleBranchEqual(T data, Instruction instruction, int index);
        protected abstract void HandleBranchNotEqual(T data, Instruction instruction, int index);
        protected abstract void HandleBranchLessThan(T data, Instruction instruction, int index);
        protected abstract void HandleBranchLessThanOrEqual(T data, Instruction instruction, int index);
        protected abstract void HandleBranchGreaterThan(T data, Instruction instruction, int index);
        protected abstract void HandleBranchGreaterThanOrEqual(T data, Instruction instruction, int index);

        protected abstract void HandleCompareEqual(T data, Instruction instruction, int index);
        protected abstract void HandleCompareNotEqual(T data, Instruction instruction, int index);
        protected abstract void HandleCompareLessThan(T data, Instruction instruction, int index);
        protected abstract void HandleCompareLessThanOrEqual(T data, Instruction instruction, int index);
        protected abstract void HandleCompareGreaterThan(T data, Instruction instruction, int index);
        protected abstract void HandleCompareGreaterThanOrEqual(T data, Instruction instruction, int index);

        protected abstract void HandleLoadNull(T data, Instruction instruction, int index);

        protected abstract void HandleNewArray(T data, Instruction instruction, int index);
        protected abstract void HandleLoadArrayLength(T data, Instruction instruction, int index);
        protected abstract void HandleLoadElement(T data, Instruction instruction, int index);
        protected abstract void HandleStoreElement(T data, Instruction instruction, int index);
    }
}
