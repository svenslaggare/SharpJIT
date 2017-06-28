using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Core;
using SharpJIT.Runtime.Memory;

namespace SharpJIT.Runtime.Frame
{
    /// <summary>
    /// Represents an entry in the call stack
    /// </summary>
    public struct CallStackEntry
    {
        /// <summary>
        /// The base pointer for the entry
        /// </summary>
        public IntPtr BasePointer { get; }

        /// <summary>
        /// The function for the entry
        /// </summary>
        public ManagedFunction Function { get; }

        /// <summary>
        /// The current instruction in the entry
        /// </summary>
        public int InstructionIndex { get; }

        /// <summary>
        /// Creates a new call stack entry
        /// </summary>
        /// <param name="basePointer">The base pointer for the frame</param>
        /// <param name="function">The function</param>
        /// <param name="instructionIndex"The current instruction in the frame</param>
        public CallStackEntry(IntPtr basePointer, ManagedFunction function, int instructionIndex)
        {
            this.BasePointer = basePointer;
            this.Function = function;
            this.InstructionIndex = instructionIndex;
        }
    }

    /// <summary>
    /// Represents a call stack
    /// </summary>
    public sealed class CallStack
    {
        private readonly MemoryManager memoryManager;
        private readonly ManagedObjectReferences managedObjectReferences;
        private readonly MemoryPage callStackMemory;

        /// <summary>
        /// The size of the call stack
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// The size of a call stack entry
        /// </summary>
        public const int CallStackEntrySize = Constants.ManagedObjectReferenceSize + sizeof(int);

        /// <summary>
        /// Creates a new call stack
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="managedObjectReferences">The object references</param>
        /// <param name="size">The maximum number of entries in the call stack</param>
        public CallStack(MemoryManager memoryManager, ManagedObjectReferences managedObjectReferences, int size)
        {
            this.memoryManager = memoryManager;
            this.managedObjectReferences = managedObjectReferences;
            this.Size = size;
            this.callStackMemory = memoryManager.CreatePage(Constants.NativePointerSize + size * CallStackEntrySize);
            NativeHelpers.SetLong(this.TopPointer, 0, this.CallStackStart.ToInt64());
        }

        /// <summary>
        /// Returns a pointer to the top value
        /// </summary>
        public IntPtr TopPointer => this.callStackMemory.Start;

        /// <summary>
        /// Returns the top value
        /// </summary>
        public IntPtr TopValue => new IntPtr(NativeHelpers.ReadLong(this.TopPointer));

        /// <summary>
        /// Returns the index of the top value on the call stack
        /// </summary>
        public int TopIndex => (int)((this.TopValue.ToInt64() - this.CallStackStart.ToInt64()) / CallStackEntrySize);

        /// <summary>
        /// Returns a pointer to the the start of the call stack
        /// </summary>
        public IntPtr CallStackStart => this.callStackMemory.Start + Constants.NativePointerSize;

        /// <summary>
        /// Returns all the entries in the call stack
        /// </summary>
        /// <param name="basePointer">The current base pointer</param>
        public IEnumerable<CallStackEntry> GetEntries(IntPtr basePointer)
        {
            var topIndex = this.TopIndex;

            for (int i = 0; i < topIndex; i++)
            {
                var entryStart = (topIndex - i) * CallStack.CallStackEntrySize;
                var callFunctionReference = NativeHelpers.ReadInt(this.CallStackStart, entryStart);
                var callInstructionIndex = NativeHelpers.ReadInt(this.CallStackStart, entryStart + Constants.ManagedObjectReferenceSize);

                var callFunction = this.managedObjectReferences.GetObject<ManagedFunction>(callFunctionReference);
                var callBasePointer = StackWalker.FindBasePointer(basePointer, 0, i);

                yield return new CallStackEntry(callBasePointer, callFunction, callInstructionIndex);
            }
        }
    }
}
