using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpAssembler.x64;
using SharpJIT.Core;
using SharpJIT.Core.Objects;
using SharpJIT.Runtime;
using SharpJIT.Runtime.Stack;
using SharpJIT.Runtime.Memory;

namespace SharpJIT.Compiler.Win64
{
    /// <summary>
    /// Represents a macro function
    /// </summary>
    /// <param name="compilationData">The function being compiled</param>
    /// <param name="instruction">The current instruction</param>
    /// <param name="instructionIndex">The index of the instruction</param>
    public delegate void MacroFunction(CompilationData compilationData, Instruction instruction, int instructionIndex);

    /// <summary>
    /// Represents a code generator
    /// </summary>
    public sealed class CodeGenerator : InstructionPass<CompilationData>
    {
        private readonly VirtualMachine virtualMachine;
        private readonly CallingConventions callingConventions = new CallingConventions();
        private readonly ExceptionHandling exceptionHandling = new ExceptionHandling();

        private readonly int stackOffset = 1;

        private readonly IDictionary<string, MacroFunction> macroFunctions = new Dictionary<string, MacroFunction>();

        /// <summary>
        /// Creates a new code generator
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public CodeGenerator(VirtualMachine virtualMachine)
        {
            this.virtualMachine = virtualMachine;
            this.exceptionHandling.GenerateHandlers(virtualMachine.MemoryManager, this.callingConventions);
        }

        /// <summary>
        /// Defines the given macro function
        /// </summary>
        /// <param name="functionDefinition">The signature of the macro</param>
        /// <param name="macroFunction">The macro function</param>
        public void DefineMacroFunction(FunctionDefinition functionDefinition, MacroFunction macroFunction)
        {
            this.macroFunctions.Add(this.virtualMachine.Binder.FunctionSignature(functionDefinition), macroFunction);
        }

        /// <summary>
        /// Generates a call to the given function
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="toCall">The address of the function to call</param>
        /// <param name="callRegister">The register where the address will be stored in</param>
        /// <param name="shadowStackNeeded">Indicates if the shadow stack is needed</param>
        private void GenerateCall(CompilationData compilationData, IntPtr toCall, Register callRegister = Register.AX, bool shadowStackNeeded = true)
        {
            var shadowStackSize = this.callingConventions.CalculateShadowStackSize();

            if (shadowStackNeeded)
            {
                compilationData.Assembler.Sub(Register.SP, shadowStackSize);
            }

            compilationData.Assembler.Move(callRegister, toCall.ToInt64());
            compilationData.Assembler.CallInRegister(callRegister);

            if (shadowStackNeeded)
            {
                compilationData.Assembler.Add(Register.SP, shadowStackSize);
            }
        }

        /// <summary>
        /// Generates a call to the given function
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="function">The function to call</param>
        /// <param name="callRegister">The register where the address will be stored in</param>
        /// <param name="shadowStackNeeded">Indicates if the shadow stack is needed</param>
        /// <typeparam name="T">The signature of the function to call</typeparam>
        private void GenerateCall<T>(CompilationData compilationData, T function, Register callRegister = Register.AX, bool shadowStackNeeded = true)
        {
            this.GenerateCall(
                compilationData,
                Marshal.GetFunctionPointerForDelegate(function));
        }

        /// <summary>
        /// Generates a call to the garbage collector
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="instruction">The current instruction</param>
        /// <param name="instructionIndex">The index of the current instruction</param>
        public void GenerateCallToGarbageCollector(CompilationData compilationData, Instruction instruction, int instructionIndex)
        {
            compilationData.Assembler.Move(IntCallingConventions.Argument0, Register.BP);
            compilationData.Assembler.Move(IntCallingConventions.Argument1, this.virtualMachine.GetManagedReference(compilationData.Function));
            compilationData.Assembler.Move(IntCallingConventions.Argument2, instructionIndex);
            compilationData.Assembler.Move(IntCallingConventions.Argument3, 0);
            this.GenerateCall<RuntimeInterface.GarbageCollectDelegate>(compilationData, RuntimeInterface.GarbageCollect);
        }

        /// <summary>
        /// Generates code for pushing the given function to the call stack
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="instructionIndex">The index of the current instruction</param>
        private void GeneratePushFunction(CompilationData compilationData, int instructionIndex)
        {
            //Get the top pointer
            var topPointer = this.virtualMachine.CallStack.TopPointer;
            compilationData.Assembler.Move(Register.AX, topPointer);
            compilationData.Assembler.Add(Register.AX, CallStack.CallStackEntrySize);

            //Check if overflow
            this.exceptionHandling.AddStackOverflowCheck(
                compilationData,
                this.virtualMachine.CallStack.CallStackStart + this.virtualMachine.CallStack.Size);

            //Store the entry
            var functionReference = this.virtualMachine.GetManagedReference(compilationData.Function);
            compilationData.Assembler.Move(Register.CX, functionReference);
            compilationData.Assembler.Move(new MemoryOperand(Register.AX, 0), Register.CX, DataSize.Size64);

            compilationData.Assembler.Move(Register.CX, instructionIndex);
            compilationData.Assembler.Move(new MemoryOperand(Register.AX, Constants.ManagedObjectReferenceSize), Register.CX, DataSize.Size64);

            //Update the top pointer
            compilationData.Assembler.Move(topPointer, Register.AX);
        }

        /// <summary>
        /// Generates code for poping a function from the calls tack
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        private void GeneratePopFunction(CompilationData compilationData)
        {
            //Get the top pointer
            var topPointer = this.virtualMachine.CallStack.TopPointer;
            compilationData.Assembler.Move(Register.AX, topPointer);
            compilationData.Assembler.Add(Register.AX, -CallStack.CallStackEntrySize);

            //Update the top pointer
            compilationData.Assembler.Move(topPointer, Register.AX);
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
            //Restore the base pointer
            compilationData.Assembler.Move(Register.SP, Register.BP);
            compilationData.Assembler.Pop(Register.BP);
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
                this.Handle(compilationData, function.Instructions[i], i);
            }
        }

        public override void Handle(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.InstructionMapping.Add(compilationData.Assembler.GeneratedCode.Count);
            base.Handle(compilationData, instruction, index);
        }

        protected override void HandlePop(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(Register.AX);
        }

        protected override void HandleLoadInt(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PushInt(instruction.IntValue);
        }

        protected override void HandleLoadFloat(CompilationData compilationData, Instruction instruction, int index)
        {
            int floatPattern = BitConverter.ToInt32(BitConverter.GetBytes(instruction.FloatValue), 0);
            compilationData.OperandStack.PushInt(floatPattern);
        }

        protected override void HandleAddInt(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(Register.CX);
            compilationData.OperandStack.PopRegister(Register.AX);
            compilationData.Assembler.Add(Register.AX, Register.CX);
            compilationData.OperandStack.PushRegister(Register.AX);
        }

        protected override void HandleSubInt(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(Register.CX);
            compilationData.OperandStack.PopRegister(Register.AX);
            compilationData.Assembler.Sub(Register.AX, Register.CX);
            compilationData.OperandStack.PushRegister(Register.AX);
        }

        protected override void HandleMulInt(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(Register.CX);
            compilationData.OperandStack.PopRegister(Register.AX);
            compilationData.Assembler.Multiply(Register.AX, Register.CX);
            compilationData.OperandStack.PushRegister(Register.AX);
        }

        protected override void HandleDivInt(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(Register.CX);
            compilationData.OperandStack.PopRegister(Register.AX);

            //Sign extend the eax register
            compilationData.Assembler.GeneratedCode.Add(0x99); //cdq
            compilationData.Assembler.Divide(Register.CX);

            compilationData.OperandStack.PushRegister(Register.AX);
        }

        protected override void HandleAddFloat(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(FloatRegister.XMM1);
            compilationData.OperandStack.PopRegister(FloatRegister.XMM0);
            compilationData.Assembler.Add(FloatRegister.XMM0, FloatRegister.XMM1);
            compilationData.OperandStack.PushRegister(FloatRegister.XMM0);
        }

        protected override void HandleSubFloat(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(FloatRegister.XMM1);
            compilationData.OperandStack.PopRegister(FloatRegister.XMM0);
            compilationData.Assembler.Sub(FloatRegister.XMM0, FloatRegister.XMM1);
            compilationData.OperandStack.PushRegister(FloatRegister.XMM0);
        }

        protected override void HandleMulFloat(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(FloatRegister.XMM1);
            compilationData.OperandStack.PopRegister(FloatRegister.XMM0);
            compilationData.Assembler.Multiply(FloatRegister.XMM0, FloatRegister.XMM1);
            compilationData.OperandStack.PushRegister(FloatRegister.XMM0);
        }

        protected override void HandleDivFloat(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(FloatRegister.XMM1);
            compilationData.OperandStack.PopRegister(FloatRegister.XMM0);
            compilationData.Assembler.Divide(FloatRegister.XMM0, FloatRegister.XMM1);
            compilationData.OperandStack.PushRegister(FloatRegister.XMM0);
        }

        protected override void HandleLoadTrue(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PushInt(1);
        }

        protected override void HandleLoadFalse(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PushInt(0);
        }

        protected override void HandleAnd(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(Register.CX);
            compilationData.OperandStack.PopRegister(Register.AX);
            compilationData.Assembler.And(Register.AX, Register.CX);
            compilationData.OperandStack.PushRegister(Register.AX);
        }

        protected override void HandleOr(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(Register.CX);
            compilationData.OperandStack.PopRegister(Register.AX);
            compilationData.Assembler.Or(Register.AX, Register.CX);
            compilationData.OperandStack.PushRegister(Register.AX);
        }

        protected override void HandleNot(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.OperandStack.PopRegister(Register.AX);
            compilationData.Assembler.Not(Register.AX);
            compilationData.Assembler.And(Register.AX, 1); //Clear the other bits, so that the value is either 0 or 1.
            compilationData.OperandStack.PushRegister(Register.AX);
        }

        /// <summary>
        /// Calculates the offset for the given local
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="localIndex">The index of the local</param>
        private int CalculateLocalOffset(CompilationData compilationData, int localIndex)
        {
            return -Assembler.RegisterSize * (stackOffset + localIndex + compilationData.FunctionDefinition.Parameters.Count);
        }

        protected override void HandleLoadLocal(CompilationData compilationData, Instruction instruction, int index)
        {
            //Load rax with the local
            var localOffset = CalculateLocalOffset(compilationData, instruction.IntValue);
            compilationData.Assembler.Move(Register.AX, new MemoryOperand(Register.BP, localOffset));

            //Push the loaded value
            compilationData.OperandStack.PushRegister(Register.AX);
        }

        protected override void HandleStoreLocal(CompilationData compilationData, Instruction instruction, int index)
        {
            //Pop the top operand
            compilationData.OperandStack.PopRegister(Register.AX);

            //Store the operand at the given local
            var localOffset = CalculateLocalOffset(compilationData, instruction.IntValue);
            compilationData.Assembler.Move(new MemoryOperand(Register.BP, localOffset), Register.AX);
        }

        /// <summary>
        /// Generates instructions for calling the given instruction
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="functionToCall">The function to call</param>
        /// <param name="instructionIndex">The index of the call instruction</param>
        private void GenerateCallInstruction(CompilationData compilationData, FunctionDefinition functionToCall, int instructionIndex)
        {
            this.GeneratePushFunction(compilationData, instructionIndex);

            //Align the stack
            int stackAlignment = this.callingConventions.CalculateStackAlignment(
                compilationData,
                functionToCall.Parameters);

            if (stackAlignment > 0)
            {
                compilationData.Assembler.Sub(Register.SP, stackAlignment);
            }

            //Set the function arguments
            this.callingConventions.CallFunctionArguments(compilationData, functionToCall);

            //Reserve 32 bytes for called function to spill registers
            var shadowStackSize = this.callingConventions.CalculateShadowStackSize();
            compilationData.Assembler.Sub(Register.SP, shadowStackSize);

            //Generate the call
            if (functionToCall.IsManaged)
            {
                //Mark that the function call needs to be patched with the entry point later
                compilationData.UnresolvedFunctionCalls.Add(new UnresolvedFunctionCall(
                    FunctionCallAddressMode.Relative,
                    functionToCall,
                    compilationData.Assembler.GeneratedCode.Count));

                compilationData.Assembler.Call(0);
            }
            else
            {
                this.GenerateCall(compilationData, functionToCall.EntryPoint, shadowStackNeeded: false);
            }

            //Unalign the stack
            compilationData.Assembler.Add(Register.SP, stackAlignment + shadowStackSize);

            //Hande the return value
            this.callingConventions.HandleReturnValue(compilationData, functionToCall);

            this.GeneratePopFunction(compilationData);
        }

        protected override void HandleCall(CompilationData compilationData, Instruction instruction, int index)
        {
            var signature = this.virtualMachine.Binder.FunctionSignature(
                instruction.StringValue,
                instruction.Parameters);

            var funcToCall = this.virtualMachine.Binder.GetFunction(signature);

            if (!this.macroFunctions.TryGetValue(signature, out var macroFunction))
            {
                this.GenerateCallInstruction(compilationData, funcToCall, index);
            }
            else
            {
                macroFunction(compilationData, instruction, index);
            }
        }

        protected override void HandleReturn(CompilationData compilationData, Instruction instruction, int index)
        {
            //compilationData.Assembler.Move(IntCallingConventions.Argument0, Register.BP);
            //compilationData.Assembler.Move(
            //    IntCallingConventions.Argument1,
            //    this.virtualMachine.ManagedObjectReferences.GetReference(compilationData.Function));
            //compilationData.Assembler.Move(IntCallingConventions.Argument2, index);

            //this.GenerateCall<RuntimeInterface.PrintCallStackDelegate>(
            //    compilationData,
            //    RuntimeInterface.PrintCallStack);

            //Handle the return value
            this.callingConventions.MakeReturnValue(compilationData);

            //Restore the base pointer
            this.CreateEpilog(compilationData);

            //Make the return
            compilationData.Assembler.Return();
        }

        protected override void HandleLoadArgument(CompilationData compilationData, Instruction instruction, int index)
        {
            //Load rax with the argument
            compilationData.Assembler.Move(
                Register.AX,
                new MemoryOperand(Register.BP, (instruction.IntValue + stackOffset) * -Assembler.RegisterSize));

            //Push the loaded value
            compilationData.OperandStack.PushRegister(Register.AX);
        }

        protected override void HandleBranch(CompilationData compilationData, Instruction instruction, int index)
        {
            compilationData.Assembler.Jump(JumpCondition.Always, 0);

            compilationData.UnresolvedBranches.Add(
                compilationData.Assembler.GeneratedCode.Count - 5,
                new UnresolvedBranchTarget(instruction.IntValue, 5));
        }

        /// <summary>
        /// Generates a comparison between the top two operands
        /// </summary>
        /// <returns>True if unsigned comparison else false</returns>
        private bool GenerateComparision(CompilationData compilationData, Instruction instruction, int index)
        {
            var opType = compilationData.Function.OperandTypes[index].Last();
            var unsignedComparison = false;

            if (opType.IsPrimitiveType(PrimitiveTypes.Float))
            {
                compilationData.OperandStack.PopRegister(FloatRegister.XMM1);
                compilationData.OperandStack.PopRegister(FloatRegister.XMM0);
                compilationData.Assembler.Compare(FloatRegister.XMM0, FloatRegister.XMM1);
                unsignedComparison = true;
            }
            else
            {
                compilationData.OperandStack.PopRegister(Register.CX);
                compilationData.OperandStack.PopRegister(Register.AX);
                compilationData.Assembler.Compare(Register.AX, Register.CX);
            }

            return unsignedComparison;
        }

        /// <summary>
        /// Handles a conditional branch
        /// </summary>
        /// <param name="condition">The condition</param>
        private void HandleConditionalBranch(CompilationData compilationData, Instruction instruction, int index, JumpCondition condition)
        {
            var unsignedComparison = this.GenerateComparision(compilationData, instruction, index);

            compilationData.Assembler.Jump(condition, 0, unsignedComparison);

            compilationData.UnresolvedBranches.Add(
                compilationData.Assembler.GeneratedCode.Count - 6,
                new UnresolvedBranchTarget(instruction.IntValue, 6));
        }

        protected override void HandleBranchEqual(CompilationData compilationData, Instruction instruction, int index)
            => this.HandleConditionalBranch(compilationData, instruction, index, JumpCondition.Equal);

        protected override void HandleBranchNotEqual(CompilationData compilationData, Instruction instruction, int index)
            => this.HandleConditionalBranch(compilationData, instruction, index, JumpCondition.NotEqual);

        protected override void HandleBranchGreaterThan(CompilationData compilationData, Instruction instruction, int index)
            => this.HandleConditionalBranch(compilationData, instruction, index, JumpCondition.GreaterThan);

        protected override void HandleBranchGreaterThanOrEqual(CompilationData compilationData, Instruction instruction, int index)
            => this.HandleConditionalBranch(compilationData, instruction, index, JumpCondition.GreaterThanOrEqual);

        protected override void HandleBranchLessThan(CompilationData compilationData, Instruction instruction, int index)
            => this.HandleConditionalBranch(compilationData, instruction, index, JumpCondition.LessThan);

        protected override void HandleBranchLessThanOrEqual(CompilationData compilationData, Instruction instruction, int index)
            => this.HandleConditionalBranch(compilationData, instruction, index, JumpCondition.LessThanOrEqual);

        /// <summary>
        /// Handles a compare instruction
        /// </summary>
        /// <param name="condition">The condition</param>
        private void HandleCompare(CompilationData compilationData, Instruction instruction, int index, JumpCondition condition)
        {
            var generatedCode = compilationData.Assembler.GeneratedCode;
            var unsignedComparison = this.GenerateComparision(compilationData, instruction, index);
            int compareJump = generatedCode.Count;
            int jump = 0;
            int trueBranchStart = 0;
            int falseBranchStart = 0;

            int target = 0;
            compilationData.Assembler.Jump(condition, target, unsignedComparison);

            //Both branches will have the same operand entry, reserve space
            compilationData.OperandStack.ReserveSpace();

            //False branch
            falseBranchStart = generatedCode.Count;
            compilationData.OperandStack.PushInt(0, false);
            jump = generatedCode.Count;
            compilationData.Assembler.Jump(JumpCondition.Always, 0);

            //True branch
            trueBranchStart = generatedCode.Count;
            compilationData.OperandStack.PushInt(1, false);

            //Set the jump targets
            NativeHelpers.SetInt(generatedCode, jump + 1, generatedCode.Count - trueBranchStart);
            NativeHelpers.SetInt(generatedCode, compareJump + 2, trueBranchStart - falseBranchStart);
        }

        protected override void HandleCompareEqual(CompilationData compilationData, Instruction instruction, int index)
            => this.HandleCompare(compilationData, instruction, index, JumpCondition.Equal);

        protected override void HandleCompareNotEqual(CompilationData compilationData, Instruction instruction, int index)
            => this.HandleCompare(compilationData, instruction, index, JumpCondition.NotEqual);

        protected override void HandleCompareGreaterThan(CompilationData compilationData, Instruction instruction, int index) 
            => this.HandleCompare(compilationData, instruction, index, JumpCondition.GreaterThan);

        protected override void HandleCompareGreaterThanOrEqual(CompilationData compilationData, Instruction instruction, int index)
            => this.HandleCompare(compilationData, instruction, index, JumpCondition.GreaterThanOrEqual);

        protected override void HandleCompareLessThan(CompilationData compilationData, Instruction instruction, int index)
            => this.HandleCompare(compilationData, instruction, index, JumpCondition.LessThan);

        protected override void HandleCompareLessThanOrEqual(CompilationData compilationData, Instruction instruction, int index)
            => this.HandleCompare(compilationData, instruction, index, JumpCondition.LessThanOrEqual);

        protected override void HandleLoadNull(CompilationData compilationData, Instruction instruction, int index)
            => compilationData.OperandStack.PushInt(0);

        protected override void HandleNewArray(CompilationData compilationData, Instruction instruction, int index)
        {
            var elementType = this.virtualMachine.TypeProvider.FindType(instruction.StringValue);
            var arrayType = this.virtualMachine.TypeProvider.FindArrayType(elementType);

            //The pointer to the type as the first arg
            compilationData.Assembler.Move(IntCallingConventions.Argument0, this.virtualMachine.GetManagedReference(arrayType));

            //Pop the size as the second arg
            compilationData.OperandStack.PopRegister(IntCallingConventions.Argument1);

            //Check that the size >= 0
            this.exceptionHandling.AddArrayCreationCheck(compilationData);

            //Call the newArray runtime function
            this.GenerateCall<RuntimeInterface.CreateArrayDelegate>(compilationData, RuntimeInterface.CreateArray);

            //Push the returned pointer
            compilationData.OperandStack.PushRegister(Register.AX);
        }

        protected override void HandleLoadArrayLength(CompilationData compilationData, Instruction instruction, int index)
        {
            //Pop the array ref
            compilationData.OperandStack.PopRegister(Register.AX);

            //Null check
            this.exceptionHandling.AddNullCheck(compilationData);

            //Get the size of the array (an int)
            compilationData.Assembler.Move(Register.AX, new MemoryOperand(Register.AX), DataSize.Size32);

            //Push the size
            compilationData.OperandStack.PushRegister(Register.AX);
        }

        /// <summary>
        /// Generates the compute address instructions
        /// </summary>
        /// <param name="elementType">The type of the element</param>
        private void GenerateComputeAddress(CompilationData compilationData, BaseType elementType)
        {
            compilationData.Assembler.Multiply(Register.R10, TypeSystem.SizeOf(elementType));
            compilationData.Assembler.Add(Register.AX, Register.R10);
            compilationData.Assembler.Add(Register.AX, Constants.ArrayLengthSize);
        }

        /// <summary>
        /// Returns the <see cref="DataSize"/> for the given size in bytes
        /// </summary>
        /// <param name="size">The size in bytes</param>
        private DataSize SizeOf(int size)
        {
            var dataSize = DataSize.Size64;
            if (size == 4)
            {
                dataSize = DataSize.Size32;
            }
            else if (size == 1)
            {
                dataSize = DataSize.Size8;
            }

            return dataSize;
        }

        protected override void HandleLoadElement(CompilationData compilationData, Instruction instruction, int index)
        {
            var elementType = this.virtualMachine.TypeProvider.FindType(instruction.StringValue);

            //Pop the operands
            compilationData.OperandStack.PopRegister(Register.R10); //The index of the element
            compilationData.OperandStack.PopRegister(Register.AX); //The address of the array

            //Error checks
            this.exceptionHandling.AddNullCheck(compilationData);
            this.exceptionHandling.AddArrayBoundsCheck(compilationData);

            //Compute the address of the element
            this.GenerateComputeAddress(compilationData, elementType);

            //Load the element
            var elementSize = TypeSystem.SizeOf(elementType);
            var dataSize = this.SizeOf(elementSize);

            var elementOffset = new MemoryOperand(Register.AX);
            if (dataSize != DataSize.Size8)
            {
                compilationData.Assembler.Move(Register.CX, elementOffset, dataSize);
            }
            else
            {
                compilationData.Assembler.Move(Register8Bit.CL, elementOffset);
            }

            compilationData.OperandStack.PushRegister(Register.CX);
        }

        protected override void HandleStoreElement(CompilationData compilationData, Instruction instruction, int index)
        {
            var elementType = this.virtualMachine.TypeProvider.FindType(instruction.StringValue);

            //Pop the operands
            compilationData.OperandStack.PopRegister(Register.DX); //The value to store
            compilationData.OperandStack.PopRegister(Register.R10); //The index of the element
            compilationData.OperandStack.PopRegister(Register.AX); //The address of the array

            //Error checks
            this.exceptionHandling.AddNullCheck(compilationData);
            this.exceptionHandling.AddArrayBoundsCheck(compilationData);

            //Compute the address of the element
            this.GenerateComputeAddress(compilationData, elementType);

            //Store the element
            var elementSize = TypeSystem.SizeOf(elementType);
            var dataSize = this.SizeOf(elementSize);

            var elementOffset = new MemoryOperand(Register.AX);
            if (dataSize != DataSize.Size8)
            {
                compilationData.Assembler.Move(elementOffset, Register.DX, dataSize);
            }
            else
            {
                compilationData.Assembler.Move(elementOffset, Register8Bit.DL);
            }

            //if (elementType.IsReference())
            //{
            //    addCardMarking(vmState, assembler, Registers::AX);
            //}
        }

        protected override void HandleNewObject(CompilationData compilationData, Instruction instruction, int index)
        {
            var classType = instruction.ClassType;
            var signature = this.virtualMachine.Binder.MemberFunctionSignature(
                classType,
                instruction.StringValue,
                instruction.Parameters);

            var constructorToCall = this.virtualMachine.Binder.GetFunction(signature);

            this.GeneratePushFunction(compilationData, index);

            //Call the newClass runtime function
            compilationData.Assembler.Move(
                IntCallingConventions.Argument0,
                this.virtualMachine.GetManagedReference(classType));

            this.GenerateCall<RuntimeInterface.CreateClassDelegate>(compilationData, RuntimeInterface.CreateClass);

            //Save the reference
            compilationData.Assembler.Move(Register.R10, Register.AX);

            //Align the stack
            int stackAlignment = this.callingConventions.CalculateStackAlignment(
                compilationData,
                constructorToCall.Parameters);

            if (stackAlignment > 0)
            {
                compilationData.Assembler.Sub(Register.SP, stackAlignment);
            }

            //Set the constructor arguments
            compilationData.Assembler.Move(IntCallingConventions.Argument0, Register.AX);
            for (int i = constructorToCall.Parameters.Count - 2; i >= 0; i--)
            {
                this.callingConventions.CallFunctionArgument(
                    compilationData,
                    i + 1,
                    instruction.Parameters[i],
                    constructorToCall);
            }

            //Push the reference to the created object
            compilationData.Assembler.Move(Register.AX, Register.R10);
            compilationData.OperandStack.PushRegister(Register.AX);

            //Reserve 32 bytes for called function to spill registers
            var shadowStackSize = this.callingConventions.CalculateShadowStackSize();
            compilationData.Assembler.Sub(Register.SP, shadowStackSize);

            //Generate the call

            //Mark that the function call needs to be patched with the entry point later
            compilationData.UnresolvedFunctionCalls.Add(new UnresolvedFunctionCall(
                FunctionCallAddressMode.Relative,
                constructorToCall,
                compilationData.Assembler.GeneratedCode.Count));

            compilationData.Assembler.Call(0);

            //Unalign the stack
            compilationData.Assembler.Add(Register.SP, stackAlignment + shadowStackSize);

            //This is for clean up after the call, as the constructor returns nothing.
            this.callingConventions.HandleReturnValue(compilationData, constructorToCall);

            this.GeneratePopFunction(compilationData);
        }

        /// <summary>
        /// Extracts the class and field from the given name
        /// </summary>
        /// <param name="name">The name</param>
        private (ClassMetadata, Field) ExtractClassAndField(string name)
        {
            TypeSystem.ExtractClassAndFieldName(name, out var className, out var fieldName);
            var classMetadata = this.virtualMachine.ClassMetadataProvider.GetMetadata(className);
            return (classMetadata, classMetadata.GetField(fieldName));
        }

        protected override void HandleLoadField(CompilationData compilationData, Instruction instruction, int index)
        {
            //Get class and field
            (var classMetadata, var field) = this.ExtractClassAndField(instruction.StringValue);
            var fieldOffset = field.LayoutOffset;
            var fieldSize = this.SizeOf(TypeSystem.SizeOf(field.Type));

            //Pop the operand
            compilationData.OperandStack.PopRegister(Register.AX); //The address of the object

            //Null check
            this.exceptionHandling.AddNullCheck(compilationData);

            //Load the field
            var fieldMemoryOperand = new MemoryOperand(Register.AX, fieldOffset);
            if (fieldSize != DataSize.Size8)
            {
                compilationData.Assembler.Move(Register.CX, fieldMemoryOperand, fieldSize);
            }
            else
            {
                compilationData.Assembler.Move(Register8Bit.CL, fieldMemoryOperand);
            }

            compilationData.OperandStack.PushRegister(Register.CX);
        }

        protected override void HandleStoreField(CompilationData compilationData, Instruction instruction, int index)
        {
            //Get class and field
            (var classMetadata, var field) = this.ExtractClassAndField(instruction.StringValue);
            var fieldOffset = field.LayoutOffset;
            var fieldSize = this.SizeOf(TypeSystem.SizeOf(field.Type));

            //Pop the operands
            compilationData.OperandStack.PopRegister(Register.DX); //The value to store
            compilationData.OperandStack.PopRegister(Register.AX); //The address of the object

            //Null check
            this.exceptionHandling.AddNullCheck(compilationData);

            //Store the field
            var fieldMemoryOperand = new MemoryOperand(Register.AX, fieldOffset);
            if (fieldSize != DataSize.Size8)
            {
                compilationData.Assembler.Move(fieldMemoryOperand, Register.DX, fieldSize);
            }
            else
            {
                compilationData.Assembler.Move(fieldMemoryOperand, Register8Bit.DL);
            }

            //Card marking
            //if (field.type()->isReference())
            //{
            //    addCardMarking(vmState, assembler, Registers::AX);
            //}
        }

        protected override void HandleCallInstance(CompilationData compilationData, Instruction instruction, int index)
        {
            var classType = instruction.ClassType;
            var signature = this.virtualMachine.Binder.MemberFunctionSignature(
                classType,
                instruction.StringValue,
                instruction.Parameters);

            var funcToCall = this.virtualMachine.Binder.GetFunction(signature);

            var firstArgOffset = new MemoryOperand(
                Register.BP,
                compilationData.OperandStack.GetStackOperandOffset(
                    compilationData.OperandStack.TopIndex - (int)funcToCall.Parameters.Count + 1));

            //Add null check
            compilationData.Assembler.Move(Register.AX, firstArgOffset);
            this.exceptionHandling.AddNullCheck(compilationData, Register.AX);

            this.GenerateCallInstruction(compilationData, funcToCall, index);
        }
    }
}
