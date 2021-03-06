﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Compiler.Win64;
using SharpJIT.Core;
using SharpJIT.Runtime.Stack;
using SharpJIT.Runtime.Memory;
using SharpJIT.Runtime.Objects;

namespace SharpJIT.Runtime
{
    /// <summary>
    /// Represents a runtime error 
    /// </summary>
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
        /// Writes the given line to the debug console
        /// </summary>
        /// <param name="line">The line</param>
        public static void DebugLog(string line)
        {
            Console.WriteLine(line);
        }

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
        public delegate IntPtr CreateArrayDelegate(int arrayTypeId, int length);

        /// <summary>
        /// Creates a new array of the given type
        /// </summary>
        /// <param name="arrayTypeId">The object id to the array type</param>
        /// <param name="length">The length of the array.</param>
        public static IntPtr CreateArray(int arrayTypeId, int length)
        {
            var arrayType = virtualMachine.GetManagedObject<ArrayType>(arrayTypeId);
            return virtualMachine.GarbageCollector.NewArray(arrayType, length);
        }

        /// <summary>
        /// Represents a delegate for the CreateClass method
        /// </summary>
        public delegate IntPtr CreateClassDelegate(int classTypeId);

        /// <summary>
        /// Creates a new instance of the given class
        /// </summary>
        /// <param name="classTypeId">The object id to the class type</param>
        public static IntPtr CreateClass(int classTypeId)
        {
            var classType = virtualMachine.GetManagedObject<ClassType>(classTypeId);
            return virtualMachine.GarbageCollector.NewClass(classType);
        }

        /// <summary>
        /// Prints the given stack frame
        /// </summary>
        /// <param name="stackFrame">The stack frame</param>
        /// <param name="printFunctionName">Indicates if the function is also printed</param>
        public static void PrintStackFrame(StackFrame stackFrame, bool printFunctionName = true)
        {
            //var indentation = "\t";
            var indentation = "    ";

            if (printFunctionName)
            {
                DebugLog($"Function: {stackFrame.Function}");
                DebugLog("");
            }

            void PrintEntry(int index, StackFrameEntry entry)
            {
                DebugLog($"{indentation}{index}: {RuntimeHelpers.ValueToString(entry.Value, entry.Type)} ({entry.Type})");
            }

            void PrintEntries(IList<StackFrameEntry> entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    PrintEntry(i, entries[i]);
                }
            }

            var arguments = stackFrame.GetArguments().ToList();
            if (arguments.Count > 0)
            {
                DebugLog(indentation + "Arguments: ");
                PrintEntries(arguments);
                DebugLog("");
            }

            var locals = stackFrame.GetLocals().ToList();
            if (locals.Count > 0)
            {
                DebugLog(indentation + "Locals: ");
                PrintEntries(locals);
                DebugLog("");
            }

            var stackOperands = stackFrame.GetStackOperands().ToList();
            if (stackOperands.Count > 0)
            {
                DebugLog(indentation + "Stack: ");
                PrintEntries(stackOperands);
                DebugLog("");
            }
        }

        /// <summary>
        /// Delegate for the <see cref="PrintCallStack(long, int, int)"/> method
        /// </summary>
        public delegate void PrintCallStackDelegate(long basePointerValue, int functionReference, int instructionIndex);

        /// <summary>
        /// Prints the call stack
        /// </summary>
        /// <param name="basePointerValue">The value of base pointer</param>
        /// <param name="functionReference">Reference to the current function</param>
        /// <param name="instructionIndex">The instruction index</param>
        public static void PrintCallStack(long basePointerValue, int functionReference, int instructionIndex)
        {
            var basePointer = new IntPtr(basePointerValue);
            var function = virtualMachine.GetManagedObject<ManagedFunction>(functionReference);

            DebugLog("------------Call stack------------");

            PrintStackFrame(new StackFrame(basePointer, function, instructionIndex));

            foreach (var callStackEntry in virtualMachine.CallStack.GetEntries(basePointer))
            {
                PrintStackFrame(new StackFrame(callStackEntry));
            }

            DebugLog("----------------------------------");
        }

        /// <summary>
        /// Delegates for the <see cref="GarbageCollect(long, int, int, int)"/> method
        /// </summary>
        public delegate void GarbageCollectDelegate(long basePointerValue, int functionReference, int instructionIndex, int generation);

        /// <summary>
        /// Collects garbage
        /// </summary>
        /// <param name="basePointerValue">The value of base pointer</param>
        /// <param name="functionReference">Reference to the current function</param>
        /// <param name="instructionIndex">The instruction index</param>
        /// <param name="generation">The generation to collect</param>
        public static void GarbageCollect(long basePointerValue, int functionReference, int instructionIndex, int generation)
        {
            var basePointer = new IntPtr(basePointerValue);
            var function = virtualMachine.GetManagedObject<ManagedFunction>(functionReference);
            virtualMachine.GarbageCollector.Collect(new StackFrame(basePointer, function, instructionIndex));
        }

        /// <summary>
        /// Stops the execution
        /// </summary>
        /// <param name="message">The error message</param>
        private static void RuntimeError(string message)
        {
            //throw new RuntimeException(message);
            //Environment.Exit(0);
            Console.WriteLine(message);
            WinAPI.ExitProcess(0);
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
