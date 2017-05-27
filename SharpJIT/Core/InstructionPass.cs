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
    public abstract class InstructionPass
    {
        /// <summary>
        /// Handles the given instruction
        /// </summary>
        /// <param name="compilationData">The complation data for the function</param>
        /// <param name="instruction">The instruction</param>
        /// <param name="index">The index of the instruction</param>
        public virtual void Handle(CompilationData compilationData, Instruction instruction, int index)
        {
            switch (instruction.OpCode)
            {
                case OpCodes.Pop:
                    this.HandlePop(compilationData, instruction, index);
                    break;
                case OpCodes.LoadInt:
                    this.HandleLoadInt(compilationData, instruction, index);
                    break;
                case OpCodes.LoadFloat:
                    this.HandleLoadFloat(compilationData, instruction, index);
                    break;
                case OpCodes.AddInt:
                    this.HandleAddInt(compilationData, instruction, index);
                    break;
                case OpCodes.SubInt:
                    this.HandleSubInt(compilationData, instruction, index);
                    break;
                case OpCodes.MulInt:
                    this.HandleMulInt(compilationData, instruction, index);
                    break;
                case OpCodes.DivInt:
                    this.HandleDivInt(compilationData, instruction, index);
                    break;
                case OpCodes.AddFloat:
                    this.HandleAddFloat(compilationData, instruction, index);
                    break;
                case OpCodes.SubFloat:
                    this.HandleSubFloat(compilationData, instruction, index);
                    break;
                case OpCodes.MulFloat:
                    this.HandleMulFloat(compilationData, instruction, index);
                    break;
                case OpCodes.DivFloat:
                    this.HandleDivFloat(compilationData, instruction, index);
                    break;
                case OpCodes.LoadTrue:
                    this.HandleLoadTrue(compilationData, instruction, index);
                    break;
                case OpCodes.LoadFalse:
                    this.HandleLoadFalse(compilationData, instruction, index);
                    break;
                case OpCodes.And:
                    this.HandleAnd(compilationData, instruction, index);
                    break;
                case OpCodes.Or:
                    this.HandleOr(compilationData, instruction, index);
                    break;
                case OpCodes.Not:
                    this.HandleNot(compilationData, instruction, index);
                    break;
                case OpCodes.Call:
                    this.HandleCall(compilationData, instruction, index);
                    break;
                case OpCodes.Return:
                    this.HandleReturn(compilationData, instruction, index);
                    break;
                case OpCodes.LoadArgument:
                    this.HandleLoadArgument(compilationData, instruction, index);
                    break;
                case OpCodes.LoadLocal:
                    this.HandleLoadLocal(compilationData, instruction, index);
                    break;
                case OpCodes.StoreLocal:
                    this.HandleStoreLocal(compilationData, instruction, index);
                    break;
                case OpCodes.Branch:
                    this.HandleBranch(compilationData, instruction, index);
                    break;
                case OpCodes.BranchEqual:
                    this.HandleBranchEqual(compilationData, instruction, index);
                    break;
                case OpCodes.BranchNotEqual:
                    this.HandleBranchNotEqual(compilationData, instruction, index);
                    break;
                case OpCodes.BranchGreaterThan:
                    this.HandleBranchGreaterThan(compilationData, instruction, index);
                    break;
                case OpCodes.BranchGreaterThanOrEqual:
                    this.HandleBranchGreaterThanOrEqual(compilationData, instruction, index);
                    break;
                case OpCodes.BranchLessThan:
                    this.HandleBranchLessThan(compilationData, instruction, index);
                    break;
                case OpCodes.BranchLessOrEqual:
                    this.HandleBranchLessThanOrEqual(compilationData, instruction, index);
                    break;
                case OpCodes.CompareEqual:
                    this.HandleCompareEqual(compilationData, instruction, index);
                    break;
                case OpCodes.CompareNotEqual:
                    this.HandleCompareNotEqual(compilationData, instruction, index);
                    break;
                case OpCodes.CompareGreaterThan:
                    this.HandleCompareGreaterThan(compilationData, instruction, index);
                    break;
                case OpCodes.CompareGreaterThanOrEqual:
                    this.HandleCompareGreaterThanOrEqual(compilationData, instruction, index);
                    break;
                case OpCodes.CompareLessThan:
                    this.HandleCompareLessThan(compilationData, instruction, index);
                    break;
                case OpCodes.CompareLessThanOrEqual:
                    this.HandleCompareLessThanOrEqual(compilationData, instruction, index);
                    break;
                case OpCodes.LoadNull:
                    this.HandleLoadNull(compilationData, instruction, index);
                    break;
                case OpCodes.NewArray:
                    this.HandleNewArray(compilationData, instruction, index);
                    break;
                case OpCodes.LoadArrayLength:
                    this.HandleLoadArrayLength(compilationData, instruction, index);
                    break;
                case OpCodes.StoreElement:
                    this.HandleStoreElement(compilationData, instruction, index);
                    break;
                case OpCodes.LoadElement:
                    this.HandleLoadElement(compilationData, instruction, index);
                    break;
            }
        }

        protected abstract void HandlePop(CompilationData compilationData, Instruction instruction, int index);

        protected abstract void HandleLoadInt(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleLoadFloat(CompilationData compilationData, Instruction instruction, int index);

        protected abstract void HandleAddInt(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleSubInt(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleMulInt(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleDivInt(CompilationData compilationData, Instruction instruction, int index);

        protected abstract void HandleAddFloat(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleSubFloat(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleMulFloat(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleDivFloat(CompilationData compilationData, Instruction instruction, int index);

        protected abstract void HandleLoadTrue(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleLoadFalse(CompilationData compilationData, Instruction instruction, int index);

        protected abstract void HandleAnd(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleOr(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleNot(CompilationData compilationData, Instruction instruction, int index);

        protected abstract void HandleLoadLocal(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleStoreLocal(CompilationData compilationData, Instruction instruction, int index);

        protected abstract void HandleCall(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleReturn(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleLoadArgument(CompilationData compilationData, Instruction instruction, int index);

        protected abstract void HandleBranch(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleBranchEqual(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleBranchNotEqual(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleBranchLessThan(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleBranchLessThanOrEqual(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleBranchGreaterThan(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleBranchGreaterThanOrEqual(CompilationData compilationData, Instruction instruction, int index);

        protected abstract void HandleCompareEqual(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleCompareNotEqual(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleCompareLessThan(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleCompareLessThanOrEqual(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleCompareGreaterThan(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleCompareGreaterThanOrEqual(CompilationData compilationData, Instruction instruction, int index);

        protected abstract void HandleLoadNull(CompilationData compilationData, Instruction instruction, int index);

        protected abstract void HandleNewArray(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleLoadArrayLength(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleLoadElement(CompilationData compilationData, Instruction instruction, int index);
        protected abstract void HandleStoreElement(CompilationData compilationData, Instruction instruction, int index);
    }
}
