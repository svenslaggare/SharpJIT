using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core;
using SharpJIT.Runtime;

namespace SharpJIT.Loader
{
    /// <summary>
    /// Represents a verifier
    /// </summary>
    public sealed class OldVerifier
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
        public OldVerifier(VirtualMachine virtualMachine)
        {
            this.virtualMachine = virtualMachine;
            this.intType = this.virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
            this.floatType = this.virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
            this.boolType = this.virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Bool);
            this.voidType = this.virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Void);
            this.nullType = this.virtualMachine.TypeProvider.FindType("Ref.Null");
        }

        /// <summary>
        /// Asserts that the given amount of operand exists on the stack.
        /// </summary>
        private void AssertOperandCount(Function function, Instruction instruction, int index, Stack<BaseType> operandStack, int count)
        {
            if (operandStack.Count < count)
            {
                throw new VerificationException(
                    $"Expected {count} operands on the stack, but got: {operandStack.Count}.",
                    function, instruction, index);
            }
        }

        /// <summary>
        /// Asserts that the given types are equal
        /// </summary>
        private void AssertSameType(Function function, Instruction instruction, int index, BaseType expectedType, BaseType actualType)
        {
            if (expectedType != actualType)
            {
                throw new VerificationException(
                    $"Expected type '{expectedType}' but got type '{actualType}'.",
                    function, instruction, index);
            }
        }

        /// <summary>
        /// Asserts that the given type exists
        /// </summary>
        /// <param name="typeName">The name of the type</param>
        /// <returns>The type</returns>
        private BaseType AssertTypeExists(Function function, Instruction instruction, int index, string typeName)
        {
            var type = this.virtualMachine.TypeProvider.FindType(typeName);
            if (type == null)
            {
                throw new VerificationException($"'{typeName}' is not a valid type.", function, instruction, index);
            }

            return type;
        }

        /// <summary>
        /// Asserts that the given type is not the void type
        /// </summary>
        /// <param name="type">The type</param>
        private void AssertNotVoidType(Function function, Instruction instruction, int index, BaseType type)
        {
            if (type == voidType)
            {
                throw new VerificationException(
                    "The void type is not allowed for this instruction",
                    function,
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
        /// <param name="function">The function being verified</param>
        /// <param name="branches">The branches</param>
        private void VerifyBranches(Function function, IList<BranchCheck> branches)
        {
            foreach (var branch in branches)
            {
                var postSourceTypes = branch.BranchTypes;
                var preTargetTypes = function.OperandTypes[branch.Target];

                if (postSourceTypes.Count == preTargetTypes.Count)
                {
                    for (int i = postSourceTypes.Count - 1; i >= 0; i--)
                    {
                        var postType = postSourceTypes[i];
                        var preType = preTargetTypes[i];

                        this.AssertSameType(
                            function,
                            function.Instructions[branch.Source],
                            branch.Source,
                            preType,
                            postType);
                    }
                }
                else
                {
                    throw new VerificationException(
                        "Expected the number of types before and after branch to be the same.",
                        function,
                        function.Instructions[branch.Source],
                        branch.Source);
                }
            }
        }

        /// <summary>
        /// Verifies the given int arithmetic operations
        /// </summary>
        private void VerifyIntArithmetic(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            this.AssertOperandCount(function, instruction, index, operandStack, 2);
            var op1 = operandStack.Pop();
            var op2 = operandStack.Pop();

            this.AssertSameType(function, instruction, index, this.intType, op1);
            this.AssertSameType(function, instruction, index, this.intType, op2);

            operandStack.Push(this.intType);
        }

        /// <summary>
        /// Verifies the given float arithmetic operations
        /// </summary>
        private void VerifyFloatArithmetic(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            this.AssertOperandCount(function, instruction, index, operandStack, 2);
            var op1 = operandStack.Pop();
            var op2 = operandStack.Pop();

            this.AssertSameType(function, instruction, index, this.floatType, op1);
            this.AssertSameType(function, instruction, index, this.floatType, op2);

            operandStack.Push(this.floatType);
        }

        /// <summary>
        /// Verifies the given load boolean instruction
        /// </summary>
        private void VerifyLoadBoolean(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            operandStack.Push(this.boolType);
        }

        /// <summary>
        /// Verifies the given binary logical operations
        /// </summary>
        private void VerifyBinaryLogicalOperators(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            this.AssertOperandCount(function, instruction, index, operandStack, 2);

            var op1 = operandStack.Pop();
            var op2 = operandStack.Pop();

            if (op1.IsPrimitiveType(PrimitiveTypes.Bool) && op2.IsPrimitiveType(PrimitiveTypes.Bool))
            {
                operandStack.Push(this.boolType);
            }
            else
            {
                throw new VerificationException(
                    $"Expected two operands of type '{this.boolType.Name}' on the stack.",
                    function,
                    instruction,
                    index);
            }
        }

        /// <summary>
        /// Verifies the given not instruction
        /// </summary>
        private void VerifyNot(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            this.AssertOperandCount(function, instruction, index, operandStack, 1);

            var op = operandStack.Pop();
            if (op.IsPrimitiveType(PrimitiveTypes.Bool))
            {
                operandStack.Push(this.boolType);
            }
            else
            {
                throw new VerificationException(
                    $"Expected one operand of type '{this.boolType.Name}' on the stack.",
                    function,
                    instruction,
                    index);
            }
        }

        /// <summary>
        /// Verifies the given call instruction
        /// </summary>
        private void VerifyCall(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
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
                    function, instruction, index);
            }

            //Check argument types
            int numParams = funcToCall.Parameters.Count;
            this.AssertOperandCount(function, instruction, index, operandStack, numParams);

            for (int argIndex = numParams - 1; argIndex >= 0; argIndex--)
            {
                var op = operandStack.Pop();
                var arg = funcToCall.Parameters[argIndex];
                this.AssertSameType(function, instruction, index, arg, op);
            }

            if (funcToCall.ReturnType != this.voidType)
            {
                operandStack.Push(funcToCall.ReturnType);
            }
        }

        /// <summary>
        /// Verifies the given conditional branch instruction
        /// </summary>
        private void VerifyConditionalBranch(Function function, Instruction instruction, int index, Stack<BaseType> operandStack, IList<BranchCheck> branches)
        {
            this.AssertOperandCount(function, instruction, index, operandStack, 2);

            //Check if valid target
            if (!(instruction.IntValue >= 0 && instruction.IntValue <= function.Instructions.Count))
            {
                throw new VerificationException(
                    $"Invalid jump target ({instruction.IntValue}).",
                    function,
                    instruction,
                    index);
            }

            var op1 = operandStack.Pop();
            var op2 = operandStack.Pop();

            if (op1 == this.intType)
            {
                if (op2.IsPrimitiveType(PrimitiveTypes.Int))
                {
                    branches.Add(new BranchCheck(index, instruction.IntValue, operandStack.ToList()));
                }
                else
                {
                    throw new VerificationException(
                        "Expected two operands of type 'Int' on the stack.",
                        function,
                        instruction,
                        index);
                }
            }
            else if (op2 == this.floatType)
            {
                if (op2.IsPrimitiveType(PrimitiveTypes.Float))
                {
                    branches.Add(new BranchCheck(index, instruction.IntValue, operandStack.ToList()));
                }
                else
                {
                    throw new VerificationException(
                        "Expected two operands of type 'Float' on the stack.",
                        function,
                        instruction,
                        index);
                }
            }
            else
            {
                throw new VerificationException(
                    "Expected two operands of comparable type on the stack.",
                    function,
                    instruction,
                    index);
            }
        }

        /// <summary>
        /// Verifies the given branch instruction
        /// </summary>
        private void VerifyBranch(Function function, Instruction instruction, int index, Stack<BaseType> operandStack, IList<BranchCheck> branches)
        {
            //Check if valid target
            if (!(instruction.IntValue >= 0 && instruction.IntValue <= function.Instructions.Count))
            {
                throw new VerificationException(
                    $"Invalid jump target ({instruction.IntValue}).",
                    function,
                    instruction,
                    index);
            }

            branches.Add(new BranchCheck(index, instruction.IntValue, operandStack.ToList()));
        }

        /// <summary>
        /// Verifies the given compare instruction
        /// </summary>
        private void VerifyCompare(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            this.AssertOperandCount(function, instruction, index, operandStack, 2);

            var op1 = operandStack.Pop();
            var op2 = operandStack.Pop();

            if (instruction.OpCode == OpCodes.CompareEqual || instruction.OpCode == OpCodes.CompareNotEqual)
            {
                if (op1 == this.intType)
                {
                    if (op2.IsPrimitiveType(PrimitiveTypes.Int))
                    {
                        operandStack.Push(this.boolType);
                    }
                    else
                    {
                        throw new VerificationException(
                            $"Expected two operands of type {this.intType.Name} on the stack.",
                            function,
                            instruction,
                            index);
                    }
                }
                else if (op1 == this.boolType)
                {
                    if (op2.IsPrimitiveType(PrimitiveTypes.Bool))
                    {
                        operandStack.Push(this.boolType);
                    }
                    else
                    {
                        throw new VerificationException(
                            $"Expected two operands of type {this.boolType.Name} on the stack.",
                            function,
                            instruction,
                            index);
                    }
                }
                else if (op1 == this.floatType)
                {
                    if (op2.IsPrimitiveType(PrimitiveTypes.Float))
                    {
                        operandStack.Push(this.boolType);
                    }
                    else
                    {
                        throw new VerificationException(
                            $"Expected two operands of type {this.floatType.Name} on the stack.",
                            function,
                            instruction,
                            index);
                    }
                }
                else if (op1 == op2)
                {
                    operandStack.Push(this.boolType);
                }
                else
                {
                    throw new VerificationException(
                        "Expected two operands of comparable type on the stack.",
                        function,
                        instruction,
                        index);
                }
            }
            else
            {
                if (op1.IsPrimitiveType(PrimitiveTypes.Int) && op2.IsPrimitiveType(PrimitiveTypes.Int))
                {
                    operandStack.Push(this.boolType);
                }
                else if (op1.IsPrimitiveType(PrimitiveTypes.Float) && op2.IsPrimitiveType(PrimitiveTypes.Float))
                {
                    operandStack.Push(this.boolType);
                }
                else
                {
                    throw new VerificationException(
                        $"Expected two operands of type {this.intType.Name} or {this.floatType.Name} on the stack.",
                        function,
                        instruction,
                        index);
                }
            }
        }

        /// <summary>
        /// Verifies the given store local instruction
        /// </summary>
        private void VerifyStoreLocal(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            this.AssertOperandCount(function, instruction, index, operandStack, 1);

            if (instruction.IntValue >= 0 && instruction.IntValue < function.Locals.Count)
            {
                var op = operandStack.Pop();
                var local = function.Locals[instruction.IntValue];
                this.AssertSameType(function, instruction, index, local, op);
            }
            else
            {
                throw new VerificationException(
                    $"Local index {instruction.IntValue} is not valid.",
                    function, instruction, index);
            }
        }

        /// <summary>
        /// Verifies the given load local instruction
        /// </summary>
        private void VerifyLoadLocal(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            if (instruction.IntValue >= 0 && instruction.IntValue < function.Locals.Count)
            {
                operandStack.Push(function.Locals[instruction.IntValue]);
            }
            else
            {
                throw new VerificationException(
                    $"Local index {instruction.IntValue} is not valid.",
                    function, instruction, index);
            }
        }

        /// <summary>
        /// Verifies the given load argument instruction
        /// </summary>
        private void VerifyLoadArgument(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            if (instruction.IntValue >= 0 && instruction.IntValue < function.Definition.Parameters.Count)
            {
                operandStack.Push(function.Definition.Parameters[instruction.IntValue]);
            }
            else
            {
                throw new VerificationException(
                    $"Argument index {instruction.IntValue} is not valid.",
                    function, instruction, index);
            }
        }

        /// <summary>
        /// Verifies the given load argument function
        /// </summary>
        private void VerifyReturn(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            int returnCount = 0;

            if (function.Definition.ReturnType != this.voidType)
            {
                returnCount = 1;
            }

            if (operandStack.Count == returnCount)
            {
                if (returnCount > 0)
                {
                    var returnType = operandStack.Pop();

                    if (returnType != function.Definition.ReturnType)
                    {
                        throw new VerificationException(
                            $"Expected return type of '{function.Definition.ReturnType}' but got type '{returnType}'.",
                            function, instruction, index);
                    }
                }
            }
            else
            {
                throw new VerificationException(
                    $"Expected {returnCount} operand(s) on the stack when returning but got {operandStack.Count} operands.",
                    function, instruction, index);
            }
        }

        /// <summary>
        /// Verifies the given load null instruction
        /// </summary>
        private void VerifyLoadNull(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            operandStack.Push(this.nullType);
        }

        /// <summary>
        /// Verifies the given create array instruction
        /// </summary>
        private void VerifyCreateArray(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            AssertOperandCount(function, instruction, index, operandStack, 1);
            AssertSameType(function, instruction, index, this.intType, operandStack.Pop());
            var elementType = AssertTypeExists(function, instruction, index, instruction.StringValue);

            if (elementType == this.voidType)
            {
                throw new VerificationException($"Arrays of type '{elementType.Name}' are not allowed.", function, instruction, index);
            }

            operandStack.Push(this.virtualMachine.TypeProvider.FindArrayType(elementType));
        }

        /// <summary>
        /// Verifies the given load array length instruction
        /// </summary>
        private void VerifyLoadArrayLength(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            AssertOperandCount(function, instruction, index, operandStack, 1);
            var arrayRefType = operandStack.Pop();

            if (!arrayRefType.IsArray() && arrayRefType != this.nullType)
            {
                throw new VerificationException(
                    "Expected operand to be an array reference.",
                    function,
                    instruction,
                    index);
            }

            operandStack.Push(this.intType);
        }

        /// <summary>
        /// Verifies the given store element instruction
        /// </summary>
        private void VerifyStoreElement(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            AssertOperandCount(function, instruction, index, operandStack, 3);

            var valueType = operandStack.Pop();
            var indexType = operandStack.Pop();
            var arrayReferenceType = operandStack.Pop();

            var isNullType = arrayReferenceType == this.nullType;

            if (!arrayReferenceType.IsArray() && !isNullType)
            {
                throw new VerificationException(
                    $"Expected first operand to be an array reference, but got type: {arrayReferenceType.Name}.",
                    function,
                    instruction,
                    index);
            }

            if (!indexType.IsPrimitiveType(PrimitiveTypes.Int))
            {
                throw new VerificationException(
                    $"Expected second operand to be of type {this.intType.Name} but got type: {indexType.Name}.",
                    function,
                    instruction,
                    index);
            }

            var elementType = AssertTypeExists(function, instruction, index, instruction.StringValue);
            AssertNotVoidType(function, instruction, index, elementType);

            if (!isNullType)
            {
                var arrayElementType = (arrayReferenceType as ArrayType).ElementType;
                AssertSameType(function, instruction, index, arrayElementType, elementType);
            }

            if (valueType != elementType)
            {
                throw new VerificationException(
                    $"Expected third operand to be of type {elementType.Name}.",
                    function,
                    instruction,
                    index);
            }
        }

        /// <summary>
        /// Verifies the given load element instruction
        /// </summary>
        private void VerifyLoadElement(Function function, Instruction instruction, int index, Stack<BaseType> operandStack)
        {
            AssertOperandCount(function, instruction, index, operandStack, 2);

            var indexType = operandStack.Pop();
            var arrayReferenceType = operandStack.Pop();

            bool isNullType = arrayReferenceType == this.nullType;

            if (!arrayReferenceType.IsArray() && !isNullType)
            {
                throw new VerificationException(
                    $"Expected first operand to be an array reference, but got type: {arrayReferenceType.Name}.",
                    function,
                    instruction,
                    index);
            }

            if (!indexType.IsPrimitiveType(PrimitiveTypes.Int))
            {
                throw new VerificationException(
                    $"Expected second operand to be of type {this.intType.Name} but got type: {indexType.Name}.",
                    function,
                    instruction,
                    index);
            }

            var elementType = this.AssertTypeExists(function, instruction, index, instruction.StringValue);
            AssertNotVoidType(function, instruction, index, elementType);

            if (!isNullType)
            {
                var arrayElementType = (arrayReferenceType as ArrayType).ElementType;
                AssertSameType(function, instruction, index, arrayElementType, elementType);
            }

            operandStack.Push(elementType);
        }

        /// <summary>
        /// Verifies the given instruction
        /// </summary>
        private void VerifyInstruction(Function function, Instruction instruction, int index, Stack<BaseType> operandStack, IList<BranchCheck> branches)
        {
            function.OperandTypes[index].AddRange(operandStack.ToList());

            switch (instruction.OpCode)
            {
                case OpCodes.Pop:
                    this.AssertOperandCount(function, instruction, index, operandStack, 1);
                    operandStack.Pop();
                    break;
                case OpCodes.LoadInt:
                    operandStack.Push(this.intType);
                    break;
                case OpCodes.LoadFloat:
                    operandStack.Push(this.floatType);
                    break;
                case OpCodes.AddInt:
                case OpCodes.SubInt:
                case OpCodes.MulInt:
                case OpCodes.DivInt:
                    VerifyIntArithmetic(function, instruction, index, operandStack);
                    break;
                case OpCodes.AddFloat:
                case OpCodes.SubFloat:
                case OpCodes.MulFloat:
                case OpCodes.DivFloat:
                    VerifyFloatArithmetic(function, instruction, index, operandStack);
                    break;
                case OpCodes.LoadTrue:
                case OpCodes.LoadFalse:
                    VerifyLoadBoolean(function, instruction, index, operandStack);
                    break;
                case OpCodes.And:
                case OpCodes.Or:
                    VerifyBinaryLogicalOperators(function, instruction, index, operandStack);
                    break;
                case OpCodes.Not:
                    VerifyNot(function, instruction, index, operandStack);
                    break;
                case OpCodes.Call:
                    VerifyCall(function, instruction, index, operandStack);
                    break;
                case OpCodes.Return:
                    VerifyReturn(function, instruction, index, operandStack);
                    break;
                case OpCodes.LoadArgument:
                    VerifyLoadArgument(function, instruction, index, operandStack);
                    break;
                case OpCodes.LoadLocal:
                    VerifyLoadLocal(function, instruction, index, operandStack);
                    break;
                case OpCodes.StoreLocal:
                    VerifyStoreLocal(function, instruction, index, operandStack);
                    break;
                case OpCodes.Branch:
                    VerifyBranch(function, instruction, index, operandStack, branches);
                    break;
                case OpCodes.BranchEqual:
                case OpCodes.BranchNotEqual:
                case OpCodes.BranchGreaterThan:
                case OpCodes.BranchGreaterThanOrEqual:
                case OpCodes.BranchLessThan:
                case OpCodes.BranchLessOrEqual:
                    VerifyConditionalBranch(function, instruction, index, operandStack, branches);
                    break;
                case OpCodes.CompareEqual:
                case OpCodes.CompareNotEqual:
                case OpCodes.CompareGreaterThan:
                case OpCodes.CompareGreaterThanOrEqual:
                case OpCodes.CompareLessThan:
                case OpCodes.CompareLessThanOrEqual:
                    VerifyCompare(function, instruction, index, operandStack);
                    break;
                case OpCodes.LoadNull:
                    VerifyLoadNull(function, instruction, index, operandStack);
                    break;
                case OpCodes.NewArray:
                    VerifyCreateArray(function, instruction, index, operandStack);
                    break;
                case OpCodes.LoadArrayLength:
                    VerifyLoadArrayLength(function, instruction, index, operandStack);
                    break;
                case OpCodes.StoreElement:
                    VerifyStoreElement(function, instruction, index, operandStack);
                    break;
                case OpCodes.LoadElement:
                    VerifyLoadElement(function, instruction, index, operandStack);
                    break;
            }
        }

        /// <summary>
        /// Verifies that given function has the correct semantics
        /// </summary>
        /// <param name="function">The function</param>
        public void VerifyFunction(Function function)
        {
            var operandStack = new Stack<BaseType>();
            var branches = new List<BranchCheck>();

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
                function.OperandStackSize = Math.Max(function.OperandStackSize, operandStack.Count);

                this.VerifyInstruction(function, instruction, i, operandStack, branches);

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

            this.VerifyBranches(function, branches);
        }
    }
}
