using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpAssembler.x64;
using SharpJIT.Core;
using SharpJIT.Runtime;

namespace SharpJIT.Compiler.Win64
{
    /// <summary>
    /// Represents a code generator
    /// </summary>
    public sealed class OldCodeGenerator
    {
        private readonly VirtualMachine virtualMachine;
        private readonly CallingConventions callingConventions = new CallingConventions();
        private readonly ExceptionHandling exceptionHandling = new ExceptionHandling();

        /// <summary>
        /// Creates a new code generator
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public OldCodeGenerator(VirtualMachine virtualMachine)
        {
            this.virtualMachine = virtualMachine;
            this.exceptionHandling.GenerateHandlers(virtualMachine.MemoryManager, this.callingConventions);
        }

        /// <summary>
        /// Generates a call to the given function
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="toCall">The address of the function to call</param>
        /// <param name="callRegister">The register where the address will be stored in</param>
        private void GenerateCall(CompilationData compilationData, IntPtr toCall, Register callRegister = Register.AX)
        {
            compilationData.Assembler.Move(callRegister, toCall.ToInt64());
            compilationData.Assembler.CallInRegister(callRegister);
        }

        /// <summary>
        /// Creates the function prolog
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        private void CreateProlog(CompilationData compilationData)
        {
            var function = compilationData.Function;
            var assembler = compilationData.Assembler;

            //Calculate the size of the stack aligned to 16 bytes
            int neededStackSize =
                (function.Definition.Parameters.Count + function.Locals.Count + compilationData.Function.OperandStackSize)
                * Assembler.RegisterSize;

            int stackSize = ((neededStackSize + 15) / 16) * 16;
            compilationData.StackSize = stackSize;

            //Save the base pointer
            assembler.Push(Register.BP);
            assembler.Move(Register.BP, Register.SP);

            //Make room for the variables on the stack
            assembler.Sub(Register.SP, stackSize);

            //Move the arguments to the stack
            this.callingConventions.MoveArgumentsToStack(compilationData);

            //Zero locals
            this.GenerateInitializeLocals(compilationData);
        }

        /// <summary>
        /// Generates code for initializing thelocals
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        private void GenerateInitializeLocals(CompilationData compilationData)
        {
            var func = compilationData.Function;
            var assembler = compilationData.Assembler;

            if (func.Locals.Count > 0)
            {
                //Zero rax
                assembler.Xor(Register.AX, Register.AX);

                for (int i = 0; i < func.Locals.Count; i++)
                {
                    int localOffset = (i + func.Definition.Parameters.Count + 1) * -Assembler.RegisterSize;
                    assembler.Move(
                        new MemoryOperand(Register.BP, localOffset),
                        Register.AX);
                }
            }
        }

        /// <summary>
        /// Creates the function epilog
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        private void CreateEpilog(CompilationData compilationData)
        {
            var assembler = compilationData.Assembler;

            //Restore the base pointer
            assembler.Move(Register.SP, Register.BP);
            assembler.Pop(Register.BP);
        }

        /// <summary>
        /// Compiles the given function
        /// </summary>
        /// <param name="function">The compilation data</param>
        public void CompileFunction(CompilationData compilationData)
        {
            var function = compilationData.Function;
            this.CreateProlog(compilationData);

            for (int i = 0; i < function.Instructions.Count; i++)
            {
                this.GenerateInstruction(compilationData, function.Instructions[i], i);
            }
        }

        /// <summary>
        /// Generates native code for the given instruction
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="instruction">The current instruction</param>
        /// <param name="index">The index of the instruction</param>
        private void GenerateInstruction(CompilationData compilationData, Instruction instruction, int index)
        {
            var generatedCode = compilationData.Function.GeneratedCode;
            var operandStack = compilationData.OperandStack;
            var funcDef = compilationData.Function.Definition;
            int stackOffset = 1;
            var assembler = compilationData.Assembler;

            compilationData.InstructionMapping.Add(generatedCode.Count);

            switch (instruction.OpCode)
            {
                case OpCodes.Pop:
                    operandStack.PopRegister(Register.AX);
                    break;
                case OpCodes.LoadInt:
                    operandStack.PushInt(instruction.IntValue);
                    break;
                case OpCodes.LoadFloat:
                    int floatPattern = BitConverter.ToInt32(BitConverter.GetBytes(instruction.FloatValue), 0);
                    operandStack.PushInt(floatPattern);
                    break;
                case OpCodes.AddInt:
                case OpCodes.SubInt:
                case OpCodes.MulInt:
                case OpCodes.DivInt:
                    operandStack.PopRegister(Register.CX);
                    operandStack.PopRegister(Register.AX);

                    switch (instruction.OpCode)
                    {
                        case OpCodes.AddInt:
                            assembler.Add(Register.AX, Register.CX);
                            break;
                        case OpCodes.SubInt:
                            assembler.Sub(Register.AX, Register.CX);
                            break;
                        case OpCodes.MulInt:
                            assembler.Multiply(Register.AX, Register.CX);
                            break;
                        case OpCodes.DivInt:
                            //Sign extends the eax register
                            generatedCode.Add(0x99); //cdq
                            assembler.Divide(Register.CX);
                            break;
                    }

                    operandStack.PushRegister(Register.AX);
                    break;
                case OpCodes.AddFloat:
                case OpCodes.SubFloat:
                case OpCodes.MulFloat:
                case OpCodes.DivFloat:
                    operandStack.PopRegister(FloatRegister.XMM1);
                    operandStack.PopRegister(FloatRegister.XMM0);

                    switch (instruction.OpCode)
                    {
                        case OpCodes.AddFloat:
                            assembler.Add(FloatRegister.XMM0, FloatRegister.XMM1);
                            break;
                        case OpCodes.SubFloat:
                            assembler.Sub(FloatRegister.XMM0, FloatRegister.XMM1);
                            break;
                        case OpCodes.MulFloat:
                            assembler.Multiply(FloatRegister.XMM0, FloatRegister.XMM1);
                            break;
                        case OpCodes.DivFloat:
                            assembler.Divide(FloatRegister.XMM0, FloatRegister.XMM1);
                            break;
                    }

                    operandStack.PushRegister(FloatRegister.XMM0);
                    break;
                case OpCodes.LoadTrue:
                    operandStack.PushInt(1);
                    break;
                case OpCodes.LoadFalse:
                    operandStack.PushInt(0);
                    break;
                case OpCodes.And:
                case OpCodes.Or:
                    {
                        //Pop 2 operands
                        operandStack.PopRegister(Register.CX);
                        operandStack.PopRegister(Register.AX);
                        bool is32bits = false;

                        //Apply the operator
                        switch (instruction.OpCode)
                        {
                            case OpCodes.And:
                                assembler.And(Register.AX, Register.CX, is32bits);
                                break;
                            case OpCodes.Or:
                                assembler.Or(Register.AX, Register.CX, is32bits);
                                break;
                            default:
                                break;
                        }

                        //Push the result
                        operandStack.PushRegister(Register.AX);
                    }
                    break;
                case OpCodes.Not:
                    operandStack.PopRegister(Register.AX);
                    assembler.Not(Register.AX);
                    assembler.And(Register.AX, 1); //Clear the other bits, so that the value is either 0 or 1.
                    operandStack.PushRegister(Register.AX);
                    break;
                case OpCodes.Call:
                    {
                        var signature = this.virtualMachine.Binder.FunctionSignature(
                            instruction.StringValue,
                            instruction.Parameters);

                        var funcToCall = this.virtualMachine.Binder.GetFunction(signature);

                        //Align the stack
                        int stackAlignment = this.callingConventions.CalculateStackAlignment(
                            compilationData,
                            funcToCall.Parameters);

                        if (stackAlignment > 0)
                        {
                            assembler.Sub(Register.SP, stackAlignment);
                        }

                        //Set the function arguments
                        this.callingConventions.CallFunctionArguments(compilationData, funcToCall);

                        //Reserve 32 bytes for called function to spill registers
                        var shadowStackSize = this.callingConventions.CalculateShadowStackSize();
                        assembler.Sub(Register.SP, shadowStackSize);

                        //Generate the call
                        if (funcToCall.IsManaged)
                        {
                            //Mark that the function call needs to be patched with the entry point later
                            compilationData.UnresolvedFunctionCalls.Add(new UnresolvedFunctionCall(
                                FunctionCallAddressMode.Relative,
                                funcToCall,
                                generatedCode.Count));

                            assembler.Call(0);
                        }
                        else
                        {
                            this.GenerateCall(compilationData, funcToCall.EntryPoint);
                        }

                        //Unalign the stack
                        assembler.Add(Register.SP, stackAlignment + shadowStackSize);

                        //Hande the return value
                        this.callingConventions.HandleReturnValue(compilationData, funcToCall);
                    }
                    break;
                case OpCodes.Return:
                    //Handle the return value
                    this.callingConventions.MakeReturnValue(compilationData);

                    //Restore the base pointer
                    this.CreateEpilog(compilationData);

                    //Make the return
                    assembler.Return();
                    break;
                case OpCodes.LoadArgument:
                    {
                        //Load rax with the argument
                        int argOffset = (instruction.IntValue + stackOffset) * -Assembler.RegisterSize;

                        assembler.Move(Register.AX, new MemoryOperand(Register.BP, argOffset));

                        //Push the loaded value
                        operandStack.PushRegister(Register.AX);
                    }
                    break;
                case OpCodes.LoadLocal:
                case OpCodes.StoreLocal:
                    {
                        //Load rax with the locals offset
                        int localOffset =
                            (stackOffset + instruction.IntValue + funcDef.Parameters.Count)
                            * -Assembler.RegisterSize;

                        if (instruction.OpCode == OpCodes.LoadLocal)
                        {
                            //Load rax with the local
                            assembler.Move(Register.AX, new MemoryOperand(Register.BP, localOffset));

                            //Push the loaded value
                            operandStack.PushRegister(Register.AX);
                        }
                        else
                        {
                            //Pop the top operand
                            operandStack.PopRegister(Register.AX);

                            //Store the operand at the given local
                            assembler.Move(new MemoryOperand(Register.BP, localOffset), Register.AX);
                        }
                    }
                    break;
                case OpCodes.Branch:
                    assembler.Jump(JumpCondition.Always, 0);

                    compilationData.UnresolvedBranches.Add(
                        generatedCode.Count - 5,
                        new UnresolvedBranchTarget(instruction.IntValue, 5));
                    break;
                case OpCodes.BranchEqual:
                case OpCodes.BranchNotEqual:
                case OpCodes.BranchGreaterThan:
                case OpCodes.BranchGreaterThanOrEqual:
                case OpCodes.BranchLessThan:
                case OpCodes.BranchLessOrEqual:
                    {
                        var opType = compilationData.Function.OperandTypes[index].Last();
                        bool unsignedComparison = false;

                        if (opType.IsPrimitiveType(PrimitiveTypes.Int))
                        {
                            operandStack.PopRegister(Register.CX);
                            operandStack.PopRegister(Register.AX);
                            assembler.Compare(Register.AX, Register.CX);
                        }
                        else if (opType.IsPrimitiveType(PrimitiveTypes.Float))
                        {
                            operandStack.PopRegister(FloatRegister.XMM1);
                            operandStack.PopRegister(FloatRegister.XMM0);
                            assembler.Compare(FloatRegister.XMM0, FloatRegister.XMM1);
                            unsignedComparison = true;
                        }

                        var condition = JumpCondition.Always;
                        switch (instruction.OpCode)
                        {
                            case OpCodes.BranchEqual:
                                condition = JumpCondition.Equal;
                                break;
                            case OpCodes.BranchNotEqual:
                                condition = JumpCondition.NotEqual;
                                break;
                            case OpCodes.BranchGreaterThan:
                                condition = JumpCondition.GreaterThan;
                                break;
                            case OpCodes.BranchGreaterThanOrEqual:
                                condition = JumpCondition.GreaterThanOrEqual;
                                break;
                            case OpCodes.BranchLessThan:
                                condition = JumpCondition.LessThan;
                                break;
                            case OpCodes.BranchLessOrEqual:
                                condition = JumpCondition.LessThanOrEqual;
                                break;
                        }

                        assembler.Jump(condition, 0, unsignedComparison);

                        compilationData.UnresolvedBranches.Add(
                            generatedCode.Count - 6,
                            new UnresolvedBranchTarget(instruction.IntValue, 6));
                    }
                    break;
                case OpCodes.CompareEqual:
                case OpCodes.CompareNotEqual:
                case OpCodes.CompareGreaterThan:
                case OpCodes.CompareGreaterThanOrEqual:
                case OpCodes.CompareLessThan:
                case OpCodes.CompareLessThanOrEqual:
                    {
                        var opType = compilationData.Function.OperandTypes[index][0];
                        bool floatOp = opType.IsPrimitiveType(PrimitiveTypes.Float);
                        bool intBasedType = !floatOp;
                        bool unsignedComparison = false;

                        //Compare
                        if (intBasedType)
                        {
                            operandStack.PopRegister(Register.CX);
                            operandStack.PopRegister(Register.AX);
                            assembler.Compare(Register.AX, Register.CX);
                        }
                        else if (floatOp)
                        {
                            operandStack.PopRegister(FloatRegister.XMM1);
                            operandStack.PopRegister(FloatRegister.XMM0);
                            assembler.Compare(FloatRegister.XMM0, FloatRegister.XMM1);
                            unsignedComparison = true;
                        }

                        //Jump
                        int compareJump = generatedCode.Count;
                        int jump = 0;
                        int trueBranchStart = 0;
                        int falseBranchStart = 0;

                        int target = 0;
                        var condition = JumpCondition.Always;
                        switch (instruction.OpCode)
                        {
                            case OpCodes.CompareEqual:
                                condition = JumpCondition.Equal;
                                break;
                            case OpCodes.CompareNotEqual:
                                condition = JumpCondition.NotEqual;
                                break;
                            case OpCodes.CompareGreaterThan:
                                condition = JumpCondition.GreaterThan;
                                break;
                            case OpCodes.CompareGreaterThanOrEqual:
                                condition = JumpCondition.GreaterThanOrEqual;
                                break;
                            case OpCodes.CompareLessThan:
                                condition = JumpCondition.LessThan;
                                break;
                            case OpCodes.CompareLessThanOrEqual:
                                condition = JumpCondition.LessThanOrEqual;
                                break;
                            default:
                                break;
                        }

                        assembler.Jump(condition, target, unsignedComparison);

                        //Both branches will have the same operand entry, reserve space
                        operandStack.ReserveSpace();
                        
                        //False branch
                        falseBranchStart = generatedCode.Count;
                        operandStack.PushInt(0, false);
                        jump = generatedCode.Count;
                        assembler.Jump(JumpCondition.Always, 0);

                        //True branch
                        trueBranchStart = generatedCode.Count;
                        operandStack.PushInt(1, false);

                        //Set the jump targets
                        NativeHelpers.SetInt(generatedCode, jump + 1, generatedCode.Count - trueBranchStart);
                        NativeHelpers.SetInt(generatedCode, compareJump + 2, trueBranchStart - falseBranchStart);
                        break;
                    }
                case OpCodes.LoadNull:
                    operandStack.PushInt(0);
                    break;
                case OpCodes.NewArray:
                    {
                        var elementType = this.virtualMachine.TypeProvider.FindType(instruction.StringValue);
                        var arrayType = this.virtualMachine.TypeProvider.FindArrayType(elementType);

                        //The pointer to the type as the first arg
                        assembler.Move(IntCallingConventions.Argument0, this.virtualMachine.ObjectReferences.GetReference(arrayType));

                        //Pop the size as the second arg
                        operandStack.PopRegister(IntCallingConventions.Argument1);

                        //Check that the size >= 0
                        this.exceptionHandling.AddArrayCreationCheck(compilationData);

                        //Call the newArray runtime function
                        RuntimeInterface.CreateArrayDelegate createArray = RuntimeInterface.CreateArray;
                        this.GenerateCall(compilationData, Marshal.GetFunctionPointerForDelegate(createArray));

                        //Push the returned pointer
                        operandStack.PushRegister(Register.AX);
                    }
                    break;
                case OpCodes.LoadArrayLength:
                    {
                        //Pop the array ref
                        operandStack.PopRegister(Register.AX);

                        //Null check
                        this.exceptionHandling.AddNullCheck(compilationData);

                        //Get the size of the array (an int)
                        assembler.Move(Register.AX, new MemoryOperand(Register.AX), DataSize.Size32);

                        //Push the size
                        operandStack.PushRegister(Register.AX);
                    }
                    break;
                case OpCodes.LoadElement:
                    {
                        var elementType = this.virtualMachine.TypeProvider.FindType(instruction.StringValue);

                        //Pop the operands
                        operandStack.PopRegister(Register.R10); //The index of the element
                        operandStack.PopRegister(Register.AX); //The address of the array

                        //Error checks
                        this.exceptionHandling.AddNullCheck(compilationData);
                        this.exceptionHandling.AddArrayBoundsCheck(compilationData);

                        //Compute the address of the element
                        assembler.Multiply(Register.R10, (int)TypeSystem.SizeOf(elementType));
                        assembler.Add(Register.AX, Register.R10);
                        assembler.Add(Register.AX, Constants.ArrayLengthSize);

                        //Load the element
                        var elementSize = TypeSystem.SizeOf(elementType);
                        var dataSize = DataSize.Size64;
                        if (elementSize == 4)
                        {
                            dataSize = DataSize.Size32;
                        }
                        else if (elementSize == 1)
                        {
                            dataSize = DataSize.Size8;
                        }

                        var elementOffset = new MemoryOperand(Register.AX);
                        if (dataSize != DataSize.Size8)
                        {
                            assembler.Move(Register.CX, elementOffset, dataSize);
                        }
                        else
                        {
                            assembler.Move(Register8Bits.CL, elementOffset);
                        }

                        operandStack.PushRegister(Register.CX);
                    }
                    break;
                case OpCodes.StoreElement:
                    {
                        var elementType = this.virtualMachine.TypeProvider.FindType(instruction.StringValue);

                        //Pop the operands
                        operandStack.PopRegister(Register.DX); //The value to store
                        operandStack.PopRegister(Register.R10); //The index of the element
                        operandStack.PopRegister(Register.AX); //The address of the array

                        //Error checks
                        this.exceptionHandling.AddNullCheck(compilationData);
                        this.exceptionHandling.AddArrayBoundsCheck(compilationData);

                        //Compute the address of the element
                        assembler.Multiply(Register.R10, TypeSystem.SizeOf(elementType));
                        assembler.Add(Register.AX, Register.R10);
                        assembler.Add(Register.AX, Constants.ArrayLengthSize);

                        //Store the element
                        var elementSize = TypeSystem.SizeOf(elementType);
                        var dataSize = DataSize.Size64;
                        if (elementSize == 4)
                        {
                            dataSize = DataSize.Size32;
                        }
                        else if (elementSize == 1)
                        {
                            dataSize = DataSize.Size8;
                        }

                        var elementOffset = new MemoryOperand(Register.AX);
                        if (dataSize != DataSize.Size8)
                        {
                            assembler.Move(elementOffset, Register.DX, dataSize);
                        }
                        else
                        {
                            assembler.Move(elementOffset, Register8Bits.DL);
                        }

                        //if (elementType.IsReference())
                        //{
                        //    addCardMarking(vmState, assembler, Registers::AX);
                        //}
                    }
                    break;
            }
        }
    }
}
