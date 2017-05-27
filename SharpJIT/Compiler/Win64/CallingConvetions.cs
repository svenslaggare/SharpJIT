using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAssembler.x64;
using SharpJIT.Core;

namespace SharpJIT.Compiler.Win64
{
    /// <summary>
    /// The float calling conventions
    /// </summary>
    public static class FloatCallingConventions
    {
        /// <summary>
        /// The first argument
        /// </summary>
        public const FloatRegister Argument0 = FloatRegister.XMM0;

        /// <summary>
        /// The second argument
        /// </summary>
        public const FloatRegister Argument1 = FloatRegister.XMM1;

        /// <summary>
        /// The third argument
        /// </summary>
        public const FloatRegister Argument2 = FloatRegister.XMM2;

        /// <summary>
        /// The fourth argument
        /// </summary>
        public const FloatRegister Argument3 = FloatRegister.XMM3;

        /// <summary>
        /// The return value register
        /// </summary>
        public const FloatRegister ReturnValue = FloatRegister.XMM0;
    }

    /// <summary>
    /// The int calling conventions
    /// </summary>
    public static class IntCallingConventions
    {
        /// <summary>
        /// The first argument
        /// </summary>
        public const Register Argument0 = Register.CX;

        /// <summary>
        /// The second argument
        /// </summary>
        public const Register Argument1 = Register.DX;

        /// <summary>
        /// The third argument
        /// </summary>
        public const Register Argument2 = Register.R8;

        /// <summary>
        /// The fourth argument
        /// </summary>
        public const Register Argument3 = Register.R9;

        /// <summary>
        /// The return value register
        /// </summary>
        public const Register ReturnValue = Register.AX;
    }

    /// <summary>
    /// Defines the calling conventions for Windows x64.
    /// </summary>
    /// <remarks>
    /// See <see href="https://en.wikipedia.org/wiki/X86_calling_conventions#Microsoft_x64_calling_convention">link<see/> for more details.
    /// </remarks>
    public sealed class CallingConventions
    {
        private static readonly int numRegisterArguments = 4;
        private readonly Register[] intArgumentRegisters = new Register[]
        {
            IntCallingConventions.Argument0,
            IntCallingConventions.Argument1,
            IntCallingConventions.Argument2,
            IntCallingConventions.Argument3
        };

        private readonly FloatRegister[] floatArgumentRegisters = new FloatRegister[]
        {
            FloatCallingConventions.Argument0,
            FloatCallingConventions.Argument1,
            FloatCallingConventions.Argument2,
            FloatCallingConventions.Argument3
        };

        /// <summary>
        /// Returns the stack argument index for the argument
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="argumentIndex">The argument index</param>
        private int GetStackArgumentIndex(CompilationData compilationData, int argumentIndex)
        {
            int stackArgIndex = 0;
            var parameterTypes = compilationData.Function.Definition.Parameters;

            int index = 0;
            foreach (var parameterType in parameterTypes)
            {
                if (index == argumentIndex)
                {
                    break;
                }

                if (index >= numRegisterArguments)
                {
                    stackArgIndex++;
                }

                index++;
            }

            return stackArgIndex;
        }

        /// <summary>
        /// Moves a none float argument to the stack
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="argumentIndex">The argument index</param>
        private void MoveNoneFloatArgumentToStack(CompilationData compilationData, int argumentIndex)
        {
            var assembler = compilationData.Assembler;
            int argStackOffset = -(1 + argumentIndex) * Assembler.RegisterSize;

            if (argumentIndex >= numRegisterArguments)
            {
                int stackArgumentIndex = this.GetStackArgumentIndex(compilationData, argumentIndex);
                int stackAlignment = this.CalculateStackAlignment(
                    compilationData,
                    compilationData.Function.Definition.Parameters);

                assembler.Move(
                    Register.AX,
                    new MemoryOperand(Register.BP, Assembler.RegisterSize * (6 + stackArgumentIndex)));

                assembler.Move(new MemoryOperand(Register.BP, argStackOffset), Register.AX);
            }
            else
            {
                assembler.Move(
                    new MemoryOperand(Register.BP, argStackOffset),
                    this.intArgumentRegisters[argumentIndex]);
            }
        }

        /// <summary>
        /// Moves a float argument to the stack
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="argumentIndex">The argument index</param>
        private void MoveFloatArgumentToStack(CompilationData compilationData, int argumentIndex)
        {
            var assembler = compilationData.Assembler;
            int argStackOffset = -(1 + argumentIndex) * Assembler.RegisterSize;

            if (argumentIndex >= numRegisterArguments)
            {
                int stackArgumentIndex = this.GetStackArgumentIndex(compilationData, argumentIndex);
                int stackAlignment = this.CalculateStackAlignment(
                    compilationData,
                    compilationData.Function.Definition.Parameters);

                assembler.Move(
                    Register.AX,
                    new MemoryOperand(
                        Register.BP,
                        Assembler.RegisterSize * (6 + stackArgumentIndex)));

                assembler.Move(new MemoryOperand(Register.BP, argStackOffset), Register.AX);
            }
            else
            {
                assembler.Move(
                    new MemoryOperand(Register.BP, argStackOffset),
                    this.floatArgumentRegisters[argumentIndex]);
            }
        }

        /// <summary>
        /// Moves the argument to the stack
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        public void MoveArgumentsToStack(CompilationData compilationData)
        {
            var function = compilationData.Function;
            var parameterTypes = function.Definition.Parameters;

            for (int argumentIndex = parameterTypes.Count - 1; argumentIndex >= 0; argumentIndex--)
            {
                if (parameterTypes[argumentIndex].IsPrimitiveType(PrimitiveTypes.Float))
                {
                    this.MoveFloatArgumentToStack(
                        compilationData,
                        argumentIndex);
                }
                else
                {
                    this.MoveNoneFloatArgumentToStack(
                        compilationData,
                        argumentIndex);
                }
            }
        }

        /// <summary>
        /// Calculates the number of argument that are passed via the stack
        /// </summary>
        /// <param name="parameterTypes">The parameter types</param>
        private int CalculateStackArguments(IReadOnlyList<BaseType> parameterTypes)
        {
            int stackArgs = 0;

            int argIndex = 0;
            foreach (var parameterType in parameterTypes)
            {
                if (argIndex >= numRegisterArguments)
                {
                    stackArgs++;
                }

                argIndex++;
            }

            return stackArgs;
        }

        /// <summary>
        /// Handles the given function call argument
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="argumentIndex">The index of the argument</param>
        /// <param name="argumentType">The type of the argument</param>
        /// <param name="toCall">The function to call</param>
        public void CallFunctionArgument(CompilationData compilationData, int argumentIndex, BaseType argumentType, FunctionDefinition toCall)
        {
            var operandStack = compilationData.OperandStack;

            //Check if to pass argument by via stack
            if (argumentIndex >= numRegisterArguments)
            {
                //Move from the operand stack to the normal stack
                operandStack.PopRegister(Register.AX);
                compilationData.Assembler.Push(Register.AX);
            }
            else
            {
                if (argumentType.IsPrimitiveType(PrimitiveTypes.Float))
                {
                    operandStack.PopRegister(this.floatArgumentRegisters[argumentIndex]);
                }
                else
                {
                    operandStack.PopRegister(this.intArgumentRegisters[argumentIndex]);
                }
            }
        }

        /// <summary>
        /// Handles the given function call arguments
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="toCall">The function to call</param>
        public void CallFunctionArguments(CompilationData compilationData, FunctionDefinition toCall)
        {
            for (int arg = toCall.Parameters.Count - 1; arg >= 0; arg--)
            {
                this.CallFunctionArgument(compilationData, arg, toCall.Parameters[arg], toCall);
            }
        }

        /// <summary>
        /// Calculates the stack alignment
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="parameters">The parameters of the function to call</param>
        public int CalculateStackAlignment(CompilationData compilationData, IReadOnlyList<BaseType> parameterTypes)
        {
            int numStackArgs = this.CalculateStackArguments(parameterTypes);
            return (numStackArgs % 2) * Assembler.RegisterSize;
        }

        /// <summary>
        /// Calculates the size of the shadow stack
        /// </summary>
        public int CalculateShadowStackSize()
        {
            return 32;
        }

        /// <summary>
        /// Makes the return value for a function
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        public void MakeReturnValue(CompilationData compilationData)
        {
            var def = compilationData.Function.Definition;

            if (!def.ReturnType.IsPrimitiveType(PrimitiveTypes.Void))
            {
                if (def.ReturnType.IsPrimitiveType(PrimitiveTypes.Float))
                {
                    compilationData.OperandStack.PopRegister(FloatCallingConventions.ReturnValue);
                }
                else
                {
                    compilationData.OperandStack.PopRegister(IntCallingConventions.ReturnValue);
                }
            }
        }

        /// <summary>
        /// Handles the return value from a function
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        /// <param name="toCall">The function to call</param>
        public void HandleReturnValue(CompilationData compilationData, FunctionDefinition toCall)
        {
            //If we have passed arguments via the stack, adjust the stack pointer.
            int numStackArgs = this.CalculateStackArguments(toCall.Parameters);

            if (numStackArgs > 0)
            {
                compilationData.Assembler.Add(
                    Register.SP,
                    numStackArgs * Assembler.RegisterSize);
            }

            if (!toCall.ReturnType.IsPrimitiveType(PrimitiveTypes.Void))
            {
                if (toCall.ReturnType.IsPrimitiveType(PrimitiveTypes.Float))
                {
                    compilationData.OperandStack.PushRegister(FloatCallingConventions.ReturnValue);
                }
                else
                {
                    compilationData.OperandStack.PushRegister(IntCallingConventions.ReturnValue);
                }
            }
        }
    }
}
