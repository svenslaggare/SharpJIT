using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Core;

namespace SharpJIT.Runtime.Stack
{
    /// <summary>
    /// Delegate for visting an object reference
    /// </summary>
    /// <param name="entry">The entry for the reference on the stack frame</param>
    public delegate void VisitObjectReference(StackFrameEntry entry);

    /// <summary>
    /// Delegate for visting a stack frame
    /// </summary>
    /// <param name="stackFrame">The stack frame</param>
    public delegate void VisitFrame(StackFrame stackFrame);

    /// <summary>
    /// Represents a stack walker
    /// </summary>
    public sealed class StackWalker
    {
        private readonly CallStack callStack;

        /// <summary>
        /// Creates a new stack walker
        /// </summary>
        /// <param name="callStack">The call stack</param>
        public StackWalker(CallStack callStack)
        {
            this.callStack = callStack;
        }

        /// <summary>
        /// Finds the base pointer for the given index
        /// </summary>
        /// <param name="currentBasePointer">The current base pointer</param>
        /// <param name="currentIndex">The current index</param>
        /// <param name="targetIndex">The target index</param>
        public static IntPtr FindBasePointer(IntPtr currentBasePointer, int currentIndex, int targetIndex)
        {
            if (currentBasePointer == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            var nextBasePointer = new IntPtr(NativeHelpers.ReadLong(currentBasePointer));
            if (currentIndex == targetIndex)
            {
                return nextBasePointer;
            }

            return FindBasePointer(nextBasePointer, currentIndex + 1, targetIndex);
        }

        /// <summary>
        /// Visits the given object reference
        /// </summary>
        /// <param name="frameEntry">The frame entry for the reference</param>
        /// <param name="visitReference">Called for valid references</param>
        private void VisitObjectReference(StackFrameEntry frameEntry, VisitObjectReference visitReference)
        {
            if (frameEntry.Type.IsReference)
            {
                //Don't visit nulls
                if (frameEntry.Value == 0)
                {
                    return;
                }

                visitReference(frameEntry);
            }
        }

        /// <summary>
        /// Visits all the object references in the given stack frame
        /// </summary>
        /// <param name="stackFrame">The stack frame</param>
        /// <param name="visitReference">Function called for each reference</param>
        private void VisitObjectReferencesInFrame(StackFrame stackFrame, VisitObjectReference visitReference)
        {
            foreach (var entry in stackFrame.GetArguments())
            {
                this.VisitObjectReference(entry, visitReference);
            }

            foreach (var entry in stackFrame.GetLocals())
            {
                this.VisitObjectReference(entry, visitReference);
            }

            foreach (var entry in stackFrame.GetStackOperands())
            {
                this.VisitObjectReference(entry, visitReference);
            }
        }

        /// <summary>
        /// Visits all the object references in all start frames, starting at the given frame
        /// </summary>
        /// <param name="stackFrame">The stack frame to start at</param>
        /// <param name="visitReference">Function called for each reference</param>
        /// <param name="visitFrame">Function called for each frame</param>
        public void VisitObjectReferences(StackFrame stackFrame, VisitObjectReference visitReference, VisitFrame visitFrame)
        {
            visitFrame?.Invoke(stackFrame);

            //Visit the calling stack frame
            this.VisitObjectReferencesInFrame(stackFrame, visitReference);

            //Then all other stack frames
            foreach (var callStackEntry in this.callStack.GetEntries(stackFrame.BasePointer))
            {
                var callStackFrame = new StackFrame(callStackEntry);
                visitFrame?.Invoke(callStackFrame);
                this.VisitObjectReferencesInFrame(callStackFrame, visitReference);
            }
        }
    }
}
