using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpAssembler.x64;
using SharpJIT.Core;
using SharpJIT.Runtime;
using SharpJIT.Runtime.Memory;

namespace SharpJIT.Compiler.Win64
{
    /// <summary>
    /// Handles exceptions
    /// </summary>
    public class ExceptionHandling
    {
        private IntPtr nullCheckHandler;
        private IntPtr arrayBoundsCheckHandler;
        private IntPtr arrayCreationCheckHandler;
        private IntPtr stackOverflowCheckHandler;

        /// <summary>
        /// Creates a call to the given handler
        /// </summary>
        /// <param name="assembler">The assembler</param>
        /// <param name="callingConventions">The calling conventions</param>
        /// <param name="handlerFunction">The handler function to call</param>
        private int CreateHandlerCall(Assembler assembler, CallingConventions callingConventions, RuntimeInterface.RuntimeErrorDelegate handlerFunction)
        {
            var handlerOffset = assembler.GeneratedCode.Count;

            int shadowSpace = callingConventions.CalculateShadowStackSize();
            if (shadowSpace > 0)
            {
                assembler.Sub(Register.SP, shadowSpace);
            }

            var handlerFunctionPointer = Marshal.GetFunctionPointerForDelegate(handlerFunction);
            assembler.Move(ExtendedRegister.R11, handlerFunctionPointer.ToInt64());
            assembler.CallInRegister(ExtendedRegister.R11);

            if (shadowSpace > 0)
            {
                assembler.Add(Register.SP, shadowSpace);
            }

            return handlerOffset;
        }

        /// <summary>
        /// Generates the exception handlers
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="callingConventions">The calling conventions</param>
        public void GenerateHandlers(MemoryManager memoryManager, CallingConventions callingConventions)
        {
            var handlerCode = new List<byte>();
            var assembler = new Assembler(handlerCode);

            //Create handler calls
            var nullHandlerOffset = this.CreateHandlerCall(assembler, callingConventions, RuntimeInterface.NullReferenceError);
            var arrayBoundsHandlerOffset = this.CreateHandlerCall(assembler, callingConventions, RuntimeInterface.ArrayOutOfBoundsError);
            var arrayCreationHandler = this.CreateHandlerCall(assembler, callingConventions, RuntimeInterface.InvalidArrayCreation);
            var stackOverflowHandler = this.CreateHandlerCall(assembler, callingConventions, RuntimeInterface.StackOverflow);

            //Allocate and copy memory
            var handlerMemory = memoryManager.AllocateCode(handlerCode.Count);
            NativeHelpers.CopyTo(handlerMemory, handlerCode);

            //Set the pointers to the handlers
            this.nullCheckHandler = handlerMemory + nullHandlerOffset;
            this.arrayBoundsCheckHandler = handlerMemory + arrayBoundsHandlerOffset;
            this.arrayCreationCheckHandler = handlerMemory + arrayCreationHandler;
            this.stackOverflowCheckHandler = handlerMemory + stackOverflowHandler;
        }

        /// <summary>
        /// Adds a null check
        /// </summary>
        /// <param name="compilationData">The function compilation data</param>
        /// <param name="refRegister">The register where the reference is located</param>
        /// <param name="compareRegister">The register where the result of the comparison will be stored</param>
        public void AddNullCheck(CompilationData compilationData, Register refRegister = Register.AX, ExtendedRegister compareRegister = ExtendedRegister.R11)
        {
            var assembler = compilationData.Assembler;

            //Compare the reference with null
            assembler.Xor(compareRegister, compareRegister); //Zero the register
            assembler.Compare(refRegister, compareRegister);

            //Jump to handler if null
            assembler.Jump(JumpCondition.Equal, 0);
            compilationData.UnresolvedNativeLabels.Add(assembler.GeneratedCode.Count - 6, this.nullCheckHandler);
        }

        /// <summary>
        /// Adds an array bounds check
        /// </summary>
        /// <param name="compilationData">The function compilation data</param>
        public void AddArrayBoundsCheck(CompilationData compilationData)
        {
            var assembler = compilationData.Assembler;

            //Get the size of the array (an int)
            assembler.Move(Register.CX, new MemoryOperand(Register.AX), DataSize.Size32);

            //Compare the index and size
            assembler.Compare(ExtendedRegister.R10, Register.CX);

            //Jump to handler if out of bounds. By using an unsigned comparison, we only need one check.
            assembler.Jump(JumpCondition.GreaterThanOrEqual, 0, true);
            compilationData.UnresolvedNativeLabels.Add(assembler.GeneratedCode.Count - 6, this.arrayBoundsCheckHandler);
        }

        /// <summary>
        /// Adds an array creation check
        /// </summary>
        /// <param name="compilationData">The function compilation data</param>
        public void AddArrayCreationCheck(CompilationData compilationData)
        {
            var assembler = compilationData.Assembler;

            assembler.Xor(ExtendedRegister.R11, ExtendedRegister.R11); //Zero the register
            assembler.Compare(ExtendedRegister.R11, IntCallingConventions.Argument1);

            //Jump to handler if invalid
            assembler.Jump(JumpCondition.GreaterThan, 0);
            compilationData.UnresolvedNativeLabels.Add(assembler.GeneratedCode.Count - 6, this.arrayCreationCheckHandler);
        }

        /// <summary>
        /// Adds a stack overflow check
        /// </summary>
        /// <param name="compilationData">The function compilation data</param>
        /// <param name="callStackEnd">The end of the call stack</param>
        public void AddStackOverflowCheck(CompilationData compilationData, IntPtr callStackEnd)
        {
            var assembler = compilationData.Assembler;

            //Move the end of the call stack to register
            assembler.Move(Register.CX, callStackEnd.ToInt64());

            //Compare the top and the end of the stack
            assembler.Compare(Register.AX, Register.CX);

            //Jump to handler if overflow
            assembler.Jump(JumpCondition.GreaterThanOrEqual, 0);
            compilationData.UnresolvedNativeLabels.Add(assembler.GeneratedCode.Count - 6, this.stackOverflowCheckHandler);
        }
    }
}
