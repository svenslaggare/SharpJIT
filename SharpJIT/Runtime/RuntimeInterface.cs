using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler.Win64;
using SharpJIT.Core;

namespace SharpJIT.Runtime
{
    /// <summary>
    /// Represents a runtime error 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class RuntimeException : Exception
    {
        /// <summary>
        /// Creates a new runtime exception
        /// </summary>
        /// <param name="message">The message</param>
        public RuntimeException(string message)
            : base(message)
        {

        }
    }

    /// <summary>
    /// Represents the interface between the managed code and the VM.
    /// </summary>
    public static class RuntimeInterface
    {
        private static VirtualMachine virtualMachine;

        /// <summary>
        /// Initializes the runtime interface using the given virtual machine.
        /// </summary>
        /// <param name="virtualMachine">The virtual machine.</param>
        public static void Initialize(VirtualMachine virtualMachine)
        {
            RuntimeInterface.virtualMachine = virtualMachine;
        }

        /// <summary>
        /// Represents a delegate for the CreateArray method
        /// </summary>
        public delegate IntPtr CreateArrayDelegate(int elementTypeId, int length);

        /// <summary>
        /// Creates a new array
        /// </summary>
        /// <param name="arrayTypeId">The object id to the array type</param>
        /// <param name="length">The length of the array.</param>
        public static IntPtr CreateArray(int arrayTypeId, int length)
        {
            var arrayType = virtualMachine.ObjectReferences.GetObject<ArrayType>(arrayTypeId);
            return virtualMachine.GarbageCollector.NewArray(arrayType, length);
        }

        /// <summary>
        /// Stops the execution
        /// </summary>
        /// <param name="message">The error message</param>
        private static void RuntimeError(string message)
        {
            //throw new RuntimeException(message);
            //Environment.Exit(0);
            WinAPI.ExitProcess(0);
            //Console.WriteLine(message);
        }

        /// <summary>
        /// Represents a delegate for a runtime error method
        /// </summary>
        public delegate void RuntimeErrorDelegate();

        /// <summary>
        /// Signals that an invalid array creation has been made
        /// </summary>
        public static void InvalidArrayCreation()
        {
            RuntimeError("The length of the array must be >= 0.");
        }

        /// <summary>
        /// Signals that an invalid array access has been made
        /// </summary>
        public static void ArrayOutOfBoundsError()
        {
            RuntimeError("Array index is out of bounds.");
        }

        /// <summary>
        /// Signals that a null reference has been made
        /// </summary>
        public static void NullReferenceError()
        {
            RuntimeError("Null reference.");
        }

        /// <summary>
        /// Signals that the call stack has run out of memory
        /// </summary>
        public static void StackOverflow()
        {
            RuntimeError("Stack overflow.");
        }
    }
}
