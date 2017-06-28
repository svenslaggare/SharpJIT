using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Core;

namespace SharpJIT.Runtime.Objects
{
    /// <summary>
    /// Represents a reference to an array
    /// </summary>
    public struct ArrayReference
    {
        /// <summary>
        /// A pointer to the first element
        /// </summary>
        public IntPtr ElementsPointer { get; }

        /// <summary>
        /// The length of the array
        /// </summary>
        public int Length { get; }

        private readonly int elementSize;

        /// <summary>
        /// Creates a new reference to the given array
        /// </summary>
        /// <param name="objectReference">A reference to the array object</param>
        public ArrayReference(ObjectReference objectReference)
        {
            this.ElementsPointer = objectReference.DataPointer + Constants.ArrayLengthSize;
            this.Length = NativeHelpers.ReadInt(objectReference.DataPointer);
            this.elementSize = TypeSystem.SizeOf(((ArrayType)objectReference.Type).ElementType);
        }

        /// <summary>
        /// Returns a pointer to the given element
        /// </summary>
        /// <param name="index">The index of the element</param>
        public IntPtr GetElement(int index)
        {
            return NativeHelpers.AddOffsetToIntPointer(this.ElementsPointer, index * this.elementSize);
        }
    }
}
