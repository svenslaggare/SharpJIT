using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler.Win64;
using SharpJIT.Core;
using SharpJIT.Runtime;

namespace SharpJIT.Loader
{
    /// <summary>
    /// Holds data for the verification of a function
    /// </summary>
    public sealed class VerifierData
    {
        /// <summary>
        /// The function to verify
        /// </summary>
        public Function Function { get; }

        /// <summary>
        /// The operand stack
        /// </summary>
        public Stack<BaseType> OperandStack { get; }

        /// <summary>
        /// The branches
        /// </summary>
        public IList<BranchCheck> Branches { get; }

        /// <summary>
        /// Creates new verifier data
        /// </summary>
        /// <param name="function">The function</param>
        public VerifierData(Function function)
        {
            this.Function = function;
            this.OperandStack = new Stack<BaseType>();
            this.Branches = new List<BranchCheck>();
        }
    }

    /// <summary>
    /// Represents a verifier
    /// </summary>
    public sealed class Verifier2 : InstructionPass<VerifierData>
    {
        private readonly VirtualMachine virtualMachine;

        private readonly BaseType intType;
        private readonly BaseType floatType;
        private readonly BaseType boolType;
        private readonly BaseType voidType;
        private readonly BaseType nullType;

        /// <summary>
        /// Creates a new verifier
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public Verifier2(VirtualMachine virtualMachine)
        {
            this.virtualMachine = virtualMachine;
            this.intType = this.virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
            this.floatType = this.virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
            this.boolType = this.virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
            this.voidType = this.virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Void);
            this.nullType = this.virtualMachine.TypeProvider.FindType("Ref.Null");
        }

        /// <summary>
        /// Throws an error
        /// </summary>
        /// <param name="verifierData">The verifier data</param>
        /// <param name="instruction">The current instruction</param>
        /// <param name="index">The current index</param>
        /// <param name="message">The message</param>
        private void ThrowError(VerifierData verifierData, Instruction instruction, int index, string message)
        {
            throw new VerificationException(
                message,
                verifierData.Function,
                instruction,
                index);
        }

        /// <summary>
        /// Asserts that the given amount of operand exists on the stack.
        /// </summary>
        private void AssertOperandCount(VerifierData verifierData, Instruction instruction, int index, int count)
        {
            if (verifierData.OperandStack.Count < count)
            {
                throw new VerificationException(
                    $"Expected {count} operands on the stack, but got: {verifierData.OperandStack.Count}.",
                    verifierData.Function,
                    instruction,
                    index);
            }
        }

        /// <summary>
        /// Asserts that the given types are equal
        /// </summary>
        private void AssertSameType(VerifierData verifierData, Instruction instruction, int index, BaseType expectedType, BaseType actualType)
        {
            if (expectedType != actualType)
            {
                throw new VerificationException(
                    $"Expected type '{expectedType}' but got type '{actualType}'.",
                    verifierData.Function,
                    instruction,
                    index);
            }
        }

        /// <summary>
        /// Asserts that the given type exists
        /// </summary>
        /// <param name="typeName">The name of the type</param>
        /// <returns>The type</returns>
        private BaseType AssertTypeExists(VerifierData verifierData, Instruction instruction, int index, string typeName)
        {
            var type = this.virtualMachine.TypeProvider.FindType(typeName);
            if (type == null)
            {
                throw new VerificationException(
                    $"'{typeName}' is not a valid type.",
                    verifierData.Function,
                    instruction,
                    index);
            }

            return type;
        }

        /// <summary>
        /// Asserts that the given type is not the void type
        /// </summary>
        /// <param name="type">The type</param>
        private void AssertNotVoidType(VerifierData verifierData, Instruction instruction, int index, BaseType type)
        {
            if (type == voidType)
            {
                throw new VerificationException(
                    "The void type is not allowed for this instruction",
                    verifierData.Function,
                    instruction,
                    index);
            }
        }

        /// <summary>
        /// Verifies the definition for the given function
        /// </summary>
        /// <param name="function">The function</param>
        private void VerifyDefinition(Function function)
        {
            foreach (var parameter in function.Definition.Parameters)
            {
                if (parameter == this.voidType)
                {
                    throw new VerificationException(
                        "'Void' is not a valid parameter type.",
                        function,
                        new Instruction(),
                        0);
                }
            }
        }

        /// <summary>
        /// Verifies the given branches
        /// </summary>
        private void VerifyBranches(VerifierData verifierData)
        {
            foreach (var branch in verifierData.Branches)
            {
                var postSourceTypes = branch.BranchTypes;
                var preTargetTypes = verifierData.Function.OperandTypes[branch.Target];

                if (postSourceTypes.Count == preTargetTypes.Count)
                {
                    for (int i = postSourceTypes.Count - 1; i >= 0; i--)
                    {
                        var postType = postSourceTypes[i];
                        var preType = preTargetTypes[i];

                        this.AssertSameType(
                            verifierData,
                            verifierData.Function.Instructions[branch.Source],
                            branch.Source,
                            preType,
                            postType);
                    }
                }
                else
                {
                    throw new VerificationException(
                        "Expected the number of types before and after branch to be the same.",
                        verifierData.Function,
                        verifierData.Function.Instructions[branch.Source],
                        branch.Source);
                }
            }
        }

        /// <summary>
        /// Verifies that given function has the correct semantics
        /// </summary>
        /// <param name="function">The function</param>
        public void VerifyFunction(Function function)
        {
            var verifierData = new VerifierData(function);

            if (function.Instructions.Count == 0)
            {
                throw new VerificationException(
                    "Empty functions are not allowed.",
                    function,
                    new Instruction(),
                    0);
            }

            this.VerifyDefinition(function);

            for (int i = 0; i < function.Instructions.Count; i++)
            {
                var instruction = function.Instructions[i];

                //Calculate the maximum size of the operand stack
                function.OperandStackSize = Math.Max(function.OperandStackSize, verifierData.OperandStack.Count);

                this.Handle(verifierData, instruction, i);

                if (i == function.Instructions.Count - 1)
                {
                    if (instruction.OpCode != OpCodes.Return)
                    {
                        throw new VerificationException(
                            "Functions must end with a return instruction.",
                            function, instruction, i);
                    }
                }
            }

            this.VerifyBranches(verifierData);
        }

        public override void Handle(VerifierData data, Instruction instruction, int index)
        {
            data.Function.OperandTypes[index].AddRange(data.OperandStack.ToList());
            base.Handle(data, instruction, index);
        }

        protected override void HandlePop(VerifierData verifierData, Instruction instruction, int index)
        {
            this.AssertOperandCount(verifierData, instruction, index, 1);
            verifierData.OperandStack.Pop();
        }

        protected override void HandleLoadInt(VerifierData verifierData, Instruction instruction, int index)
        {
            verifierData.OperandStack.Push(this.intType);
        }

        protected override void HandleLoadFloat(VerifierData verifierData, Instruction instruction, int index)
        {
            verifierData.OperandStack.Push(this.floatType);
        }

        /// <summary>
        /// Verifies the given int arithmetic operations
        /// </summary>
        private void VerifyIntArithmetic(VerifierData verifierData, Instruction instruction, int index)
        {
            this.AssertOperandCount(verifierData, instruction, index, 2);
            var op1 = verifierData.OperandStack.Pop();
            var op2 = verifierData.OperandStack.Pop();

            this.AssertSameType(verifierData, instruction, index, this.intType, op1);
            this.AssertSameType(verifierData, instruction, index, this.intType, op2);

            verifierData.OperandStack.Push(this.intType);
        }

        protected override void HandleAddInt(VerifierData verifierData, Instruction instruction, int index)
            => this.VerifyIntArithmetic(verifierData, instruction, index);

        protected override void HandleSubInt(VerifierData verifierData, Instruction instruction, int index)
            => this.VerifyIntArithmetic(verifierData, instruction, index);

        protected override void HandleMulInt(VerifierData verifierData, Instruction instruction, int index)
            => this.VerifyIntArithmetic(verifierData, instruction, index);

        protected override void HandleDivInt(VerifierData verifierData, Instruction instruction, int index)
            => this.VerifyIntArithmetic(verifierData, instruction, index);

        /// <summary>
        /// Verifies the given float arithmetic operations
        /// </summary>
        private void VerifyFloatArithmetic(VerifierData verifierData, Instruction instruction, int index)
        {
            this.AssertOperandCount(verifierData, instruction, index, 2);
            var op1 = verifierData.OperandStack.Pop();
            var op2 = verifierData.OperandStack.Pop();

            this.AssertSameType(verifierData, instruction, index, this.floatType, op1);
            this.AssertSameType(verifierData, instruction, index, this.floatType, op2);

            verifierData.OperandStack.Push(this.floatType);
        }

        protected override void HandleAddFloat(VerifierData verifierData, Instruction instruction, int index)
            => this.VerifyFloatArithmetic(verifierData, instruction, index);

        protected override void HandleSubFloat(VerifierData verifierData, Instruction instruction, int index)
            => this.VerifyFloatArithmetic(verifierData, instruction, index);

        protected override void HandleMulFloat(VerifierData verifierData, Instruction instruction, int index)
            => this.VerifyFloatArithmetic(verifierData, instruction, index);

        protected override void HandleDivFloat(VerifierData verifierData, Instruction instruction, int index)
            => this.VerifyFloatArithmetic(verifierData, instruction, index);

        protected override void HandleLoadTrue(VerifierData verifierData, Instruction instruction, int index)
        {
            verifierData.OperandStack.Push(this.boolType);
        }

        protected override void HandleLoadFalse(VerifierData verifierData, Instruction instruction, int index)
        {
            verifierData.OperandStack.Push(this.boolType);
        }

        /// <summary>
        /// Verifies the given binary logical operations
        /// </summary>
        private void VerifyBinaryLogicalOperators(VerifierData verifierData, Instruction instruction, int index)
        {
            this.AssertOperandCount(verifierData, instruction, index, 2);
        
            var op1 = verifierData.OperandStack.Pop();
            var op2 = verifierData.OperandStack.Pop();

            if (op1.IsPrimitiveType(PrimitiveTypes.Bool) && op2.IsPrimitiveType(PrimitiveTypes.Bool))
            {
                verifierData.OperandStack.Push(this.boolType);
            }
            else
            {
                throw new VerificationException(
                    $"Expected two operands of type '{this.boolType.Name}' on the stack.",
                    verifierData.Function,
                    instruction,
                    index);
            }
        }

        protected override void HandleAnd(VerifierData verifierData, Instruction instruction, int index)
            => this.VerifyBinaryLogicalOperators(verifierData, instruction, index);

        protected override void HandleOr(VerifierData verifierData, Instruction instruction, int index)
            => this.VerifyBinaryLogicalOperators(verifierData, instruction, index);

        protected override void HandleNot(VerifierData verifierData, Instruction instruction, int index)
        {
            this.AssertOperandCount(verifierData, instruction, index, 1);

            var op = verifierData.OperandStack.Pop();
            if (op.IsPrimitiveType(PrimitiveTypes.Bool))
            {
                verifierData.OperandStack.Push(this.boolType);
            }
            else
            {
                throw new VerificationException(
                    $"Expected one operand of type '{this.boolType.Name}' on the stack.",
                    verifierData.Function,
                    instruction,
                    index);
            }
        }

        protected override void HandleLoadLocal(VerifierData verifierData, Instruction instruction, int index)
        {
            var locals = verifierData.Function.Locals;
            if (instruction.IntValue >= 0 && instruction.IntValue < locals.Count)
            {
                verifierData.OperandStack.Push(locals[instruction.IntValue]);
            }
            else
            {
                throw new VerificationException(
                    $"Local index {instruction.IntValue} is not valid.",
                    verifierData.Function,
                    instruction,
                    index);
            }
        }

        protected override void HandleStoreLocal(VerifierData verifierData, Instruction instruction, int index)
        {
            this.AssertOperandCount(verifierData, instruction, index, 1);
            var locals = verifierData.Function.Locals;

            if (instruction.IntValue >= 0 && instruction.IntValue < locals.Count)
            {
                var op = verifierData.OperandStack.Pop();
                var local = locals[instruction.IntValue];
                this.AssertSameType(verifierData, instruction, index, local, op);
            }
            else
            {
                throw new VerificationException(
                    $"Local index {instruction.IntValue} is not valid.",
                    verifierData.Function,
                    instruction,
                    index);
            }
        }

        protected override void HandleCall(VerifierData verifierData, Instruction instruction, int index)
        {
            var signature = this.virtualMachine.Binder.FunctionSignature(
                instruction.StringValue,
                instruction.Parameters);

            var funcToCall = this.virtualMachine.Binder.GetFunction(signature);

            //Check that the function exists
            if (funcToCall == null)
            {
                throw new VerificationException(
                    $"There exists no function with the signature '{signature}'.",
                    verifierData.Function,
                    instruction,
                    index);
            }

            //Check argument types
            int numParams = funcToCall.Parameters.Count;
            this.AssertOperandCount(verifierData, instruction, index, numParams);

            for (int argIndex = numParams - 1; argIndex >= 0; argIndex--)
            {
                var op = verifierData.OperandStack.Pop();
                var arg = funcToCall.Parameters[argIndex];
                this.AssertSameType(verifierData, instruction, index, arg, op);
            }

            if (funcToCall.ReturnType != this.voidType)
            {
                verifierData.OperandStack.Push(funcToCall.ReturnType);
            }
        }

        protected override void HandleReturn(VerifierData verifierData, Instruction instruction, int index)
        {
            int returnCount = 0;
            var functionDefinition = verifierData.Function.Definition;

            if (functionDefinition.ReturnType != this.voidType)
            {
                returnCount = 1;
            }

            if (verifierData.OperandStack.Count == returnCount)
            {
                if (returnCount > 0)
                {
                    var returnType = verifierData.OperandStack.Pop();

                    if (returnType != functionDefinition.ReturnType)
                    {
                        throw new VerificationException(
                            $"Expected return type of '{functionDefinition.ReturnType}' but got type '{returnType}'.",
                            verifierData.Function,
                            instruction,
                            index);
                    }
                }
            }
            else
            {
                throw new VerificationException(
                    $"Expected {returnCount} operand(s) on the stack when returning but got {verifierData.OperandStack.Count} operands.",
                    verifierData.Function,
                    instruction,
                    index);
            }
        }

        protected override void HandleLoadArgument(VerifierData verifierData, Instruction instruction, int index)
        {
            var functionDefinition = verifierData.Function.Definition;
            if (instruction.IntValue >= 0 && instruction.IntValue < functionDefinition.Parameters.Count)
            {
                verifierData.OperandStack.Push(functionDefinition.Parameters[instruction.IntValue]);
            }
            else
            {
                throw new VerificationException(
                    $"Argument index {instruction.IntValue} is not valid.",
                    verifierData.Function,
                    instruction,
                    index);
            }
        }

        protected override void HandleBranch(VerifierData verifierData, Instruction instruction, int index)
        {
            //Check if valid target
            if (!(instruction.IntValue >= 0 && instruction.IntValue <= verifierData.Function.Instructions.Count))
            {
                throw new VerificationException(
                    $"Invalid jump target ({instruction.IntValue}).",
                    verifierData.Function,
                    instruction,
                    index);
            }

            verifierData.Branches.Add(new BranchCheck(index, instruction.IntValue, verifierData.OperandStack.ToList()));
        }


        /// <summary>
        /// Verifies the given conditional branch instruction
        /// </summary>
        private void HandleConditionalBranch(VerifierData verifierData, Instruction instruction, int index)
        {
            this.AssertOperandCount(verifierData, instruction, index, 2);

            //Check if valid target
            if (!(instruction.IntValue >= 0 && instruction.IntValue <= verifierData.Function.Instructions.Count))
            {
                throw new VerificationException(
                    $"Invalid jump target ({instruction.IntValue}).",
                    verifierData.Function,
                    instruction,
                    index);
            }

            var op1 = verifierData.OperandStack.Pop();
            var op2 = verifierData.OperandStack.Pop();

            if (op1 == this.intType)
            {
                if (op2.IsPrimitiveType(PrimitiveTypes.Int))
                {
                    verifierData.Branches.Add(new BranchCheck(index, instruction.IntValue, verifierData.OperandStack.ToList()));
                }
                else
                {
                    throw new VerificationException(
                        "Expected two operands of type 'Int' on the stack.",
                        verifierData.Function,
                        instruction,
                        index);
                }
            }
            else if (op2 == this.floatType)
            {
                if (op2.IsPrimitiveType(PrimitiveTypes.Float))
                {
                    verifierData.Branches.Add(new BranchCheck(index, instruction.IntValue, verifierData.OperandStack.ToList()));
                }
                else
                {
                    throw new VerificationException(
                        "Expected two operands of type 'Float' on the stack.",
                        verifierData.Function,
                        instruction,
                        index);
                }
            }
            else
            {
                throw new VerificationException(
                    "Expected two operands of comparable type on the stack.",
                    verifierData.Function,
                    instruction,
                    index);
            }
        }

        protected override void HandleBranchEqual(VerifierData verifierData, Instruction instruction, int index)
            => this.HandleConditionalBranch(verifierData, instruction, index);

        protected override void HandleBranchNotEqual(VerifierData verifierData, Instruction instruction, int index)
            => this.HandleConditionalBranch(verifierData, instruction, index);

        protected override void HandleBranchGreaterThan(VerifierData verifierData, Instruction instruction, int index)
            => this.HandleConditionalBranch(verifierData, instruction, index);

        protected override void HandleBranchGreaterThanOrEqual(VerifierData verifierData, Instruction instruction, int index)
            => this.HandleConditionalBranch(verifierData, instruction, index);

        protected override void HandleBranchLessThan(VerifierData verifierData, Instruction instruction, int index)
            => this.HandleConditionalBranch(verifierData, instruction, index);

        protected override void HandleBranchLessThanOrEqual(VerifierData verifierData, Instruction instruction, int index)
            => this.HandleConditionalBranch(verifierData, instruction, index);

        /// <summary>
        /// Verifies the given compare instruction
        /// </summary>
        private void HandleCompare(VerifierData verifierData, Instruction instruction, int index)
        {
            this.AssertOperandCount(verifierData, instruction, index, 2);

            var op1 = verifierData.OperandStack.Pop();
            var op2 = verifierData.OperandStack.Pop();

            if (instruction.OpCode == OpCodes.CompareEqual || instruction.OpCode == OpCodes.CompareNotEqual)
            {
                if (op1 == this.intType)
                {
                    if (op2.IsPrimitiveType(PrimitiveTypes.Int))
                    {
                        verifierData.OperandStack.Push(this.boolType);
                    }
                    else
                    {
                        throw new VerificationException(
                            $"Expected two operands of type {this.intType.Name} on the stack.",
                            verifierData.Function,
                            instruction,
                            index);
                    }
                }
                else if (op1 == this.boolType)
                {
                    if (op2.IsPrimitiveType(PrimitiveTypes.Bool))
                    {
                        verifierData.OperandStack.Push(this.boolType);
                    }
                    else
                    {
                        throw new VerificationException(
                            $"Expected two operands of type {this.boolType.Name} on the stack.",
                            verifierData.Function,
                            instruction,
                            index);
                    }
                }
                else if (op1 == this.floatType)
                {
                    if (op2.IsPrimitiveType(PrimitiveTypes.Float))
                    {
                        verifierData.OperandStack.Push(this.boolType);
                    }
                    else
                    {
                        throw new VerificationException(
                            $"Expected two operands of type {this.floatType.Name} on the stack.",
                            verifierData.Function,
                            instruction,
                            index);
                    }
                }
                else if (op1 == op2)
                {
                    verifierData.OperandStack.Push(this.boolType);
                }
                else
                {
                    throw new VerificationException(
                        "Expected two operands of comparable type on the stack.",
                        verifierData.Function,
                        instruction,
                        index);
                }
            }
            else
            {
                if (op1.IsPrimitiveType(PrimitiveTypes.Int) && op2.IsPrimitiveType(PrimitiveTypes.Int))
                {
                    verifierData.OperandStack.Push(this.boolType);
                }
                else if (op1.IsPrimitiveType(PrimitiveTypes.Float) && op2.IsPrimitiveType(PrimitiveTypes.Float))
                {
                    verifierData.OperandStack.Push(this.boolType);
                }
                else
                {
                    throw new VerificationException(
                        $"Expected two operands of type {this.intType.Name} or {this.floatType.Name} on the stack.",
                        verifierData.Function,
                        instruction,
                        index);
                }
            }
        }

        protected override void HandleCompareEqual(VerifierData verifierData, Instruction instruction, int index)
            => this.HandleCompare(verifierData, instruction, index);

        protected override void HandleCompareNotEqual(VerifierData verifierData, Instruction instruction, int index)
            => this.HandleCompare(verifierData, instruction, index);

        protected override void HandleCompareGreaterThan(VerifierData verifierData, Instruction instruction, int index)
            => this.HandleCompare(verifierData, instruction, index);

        protected override void HandleCompareGreaterThanOrEqual(VerifierData verifierData, Instruction instruction, int index)
           => this.HandleCompare(verifierData, instruction, index);

        protected override void HandleCompareLessThan(VerifierData verifierData, Instruction instruction, int index)
            => this.HandleCompare(verifierData, instruction, index);

        protected override void HandleCompareLessThanOrEqual(VerifierData verifierData, Instruction instruction, int index)
            => this.HandleCompare(verifierData, instruction, index);

        protected override void HandleLoadNull(VerifierData verifierData, Instruction instruction, int index)
        {
            verifierData.OperandStack.Push(this.nullType);
        }

        protected override void HandleNewArray(VerifierData verifierData, Instruction instruction, int index)
        {
            AssertOperandCount(verifierData, instruction, index, 1);
            AssertSameType(verifierData, instruction, index, this.intType, verifierData.OperandStack.Pop());
            var elementType = AssertTypeExists(verifierData, instruction, index, instruction.StringValue);

            if (elementType == this.voidType)
            {
                throw new VerificationException(
                    $"Arrays of type '{elementType.Name}' are not allowed.",
                    verifierData.Function,
                    instruction,
                    index);
            }

            verifierData.OperandStack.Push(this.virtualMachine.TypeProvider.FindArrayType(elementType));
        }

        protected override void HandleLoadArrayLength(VerifierData verifierData, Instruction instruction, int index)
        {
            AssertOperandCount(verifierData, instruction, index, 1);
            var arrayRefType = verifierData.OperandStack.Pop();

            if (!arrayRefType.IsArray() && arrayRefType != this.nullType)
            {
                throw new VerificationException(
                    "Expected operand to be an array reference.",
                    verifierData.Function,
                    instruction,
                    index);
            }

            verifierData.OperandStack.Push(this.intType);
        }

        protected override void HandleLoadElement(VerifierData verifierData, Instruction instruction, int index)
        {
            AssertOperandCount(verifierData, instruction, index, 2);

            var indexType = verifierData.OperandStack.Pop();
            var arrayReferenceType = verifierData.OperandStack.Pop();

            bool isNullType = arrayReferenceType == this.nullType;

            if (!arrayReferenceType.IsArray() && !isNullType)
            {
                throw new VerificationException(
                    $"Expected first operand to be an array reference, but got type: {arrayReferenceType.Name}.",
                    verifierData.Function,
                    instruction,
                    index);
            }

            if (!indexType.IsPrimitiveType(PrimitiveTypes.Int))
            {
                throw new VerificationException(
                    $"Expected second operand to be of type {this.intType.Name} but got type: {indexType.Name}.",
                    verifierData.Function,
                    instruction,
                    index);
            }

            var elementType = this.AssertTypeExists(verifierData, instruction, index, instruction.StringValue);
            AssertNotVoidType(verifierData, instruction, index, elementType);

            if (!isNullType)
            {
                var arrayElementType = (arrayReferenceType as ArrayType).ElementType;
                AssertSameType(verifierData, instruction, index, arrayElementType, elementType);
            }

            verifierData.OperandStack.Push(elementType);
        }

        protected override void HandleStoreElement(VerifierData verifierData, Instruction instruction, int index)
        {
            AssertOperandCount(verifierData, instruction, index, 3);

            var valueType = verifierData.OperandStack.Pop();
            var indexType = verifierData.OperandStack.Pop();
            var arrayReferenceType = verifierData.OperandStack.Pop();

            var isNullType = arrayReferenceType == this.nullType;

            if (!arrayReferenceType.IsArray() && !isNullType)
            {
                throw new VerificationException(
                    $"Expected first operand to be an array reference, but got type: {arrayReferenceType.Name}.",
                    verifierData.Function,
                    instruction,
                    index);
            }

            if (!indexType.IsPrimitiveType(PrimitiveTypes.Int))
            {
                throw new VerificationException(
                    $"Expected second operand to be of type {this.intType.Name} but got type: {indexType.Name}.",
                    verifierData.Function,
                    instruction,
                    index);
            }

            var elementType = AssertTypeExists(verifierData, instruction, index, instruction.StringValue);
            AssertNotVoidType(verifierData, instruction, index, elementType);

            if (!isNullType)
            {
                var arrayElementType = (arrayReferenceType as ArrayType).ElementType;
                AssertSameType(verifierData, instruction, index, arrayElementType, elementType);
            }

            if (valueType != elementType)
            {
                throw new VerificationException(
                    $"Expected third operand to be of type {elementType.Name}.",
                    verifierData.Function,
                    instruction,
                    index);
            }
        }
    }
}
