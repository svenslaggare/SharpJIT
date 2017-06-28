using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Core;
using SharpJIT.Runtime.Memory;

namespace SharpJIT.Runtime.Objects
{
    /// <summary>
    /// Represents an object reference
    /// </summary>
    public struct ObjectReference
    {
        private readonly IntPtr pointer;

        /// <summary>
        /// The type of the object
        /// </summary>
        public BaseType Type { get; }

        /// <summary>
        /// The size of the object
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Creates a new object reference for the given object
        /// </summary>
        /// <param name="managedObjectReferences">The managed object references</param>
        /// <param name="pointer">A pointer to the start of the data part of the object</param>
        public ObjectReference(ManagedObjectReferences managedObjectReferences, IntPtr pointer)
        {
            this.pointer = pointer - Constants.ObjectHeaderSize;
            var typeId = NativeHelpers.ReadInt(this.pointer);
            this.Type = managedObjectReferences.GetObject<BaseType>(typeId);

            if (this.Type.IsArray)
            {
                var arrayType = (ArrayType)this.Type;
                var elementSize = TypeSystem.SizeOf(arrayType.ElementType);
                var length = NativeHelpers.ReadInt(this.pointer + Constants.ObjectHeaderSize);
                this.Size = Constants.ArrayLengthSize + (length * elementSize);
            }
            else
            {
                this.Size = ((ClassType)this.Type).Metadata.Size;
            }
        }

        /// <summary>
        /// Returns a pointer to the start of the data part of the object
        /// </summary>
        public IntPtr DataPointer => this.pointer + Constants.ObjectHeaderSize;

        /// <summary>
        /// Returns a pointer to the start of the object
        /// </summary>
        public IntPtr FullPointer => this.pointer;

        /// <summary>
        /// Returns the full size of this object (header + data)
        /// </summary>
        public int FullSize => Constants.ObjectHeaderSize + this.Size;

        /// <summary>
        /// Indicates if the object is marked
        /// </summary>
        public bool IsMarked => (NativeHelpers.ReadByte(this.pointer, Constants.ManagedObjectReferenceSize) & 0x1) == 1 ? true : false;

        /// <summary>
        /// Returns the number of collections that the object has survived
        /// </summary>
        public int SurvivalCount => (NativeHelpers.ReadByte(this.pointer, Constants.ManagedObjectReferenceSize) >> 1) & 0x7f;

        /// <summary>
        /// Sets the information for the garbage collector
        /// </summary>
        /// <param name="isMarked">Indicates if the object is marked</param>
        /// <param name="survivalCount">The number of collections that the object has survived</param>
        public void SetGarbageCollectorInformation(bool isMarked, int survivalCount)
        {
            int gcInfo = (isMarked ? 1 : 0) | (survivalCount << 1);
            NativeHelpers.SetByte(this.pointer, Constants.ManagedObjectReferenceSize, (byte)gcInfo);
        }

        /// <summary>
        /// Sets if the object is marked or not
        /// </summary>
        /// <param name="isMarked">Indicates if the object is marked</param>
        public void SetMarked(bool isMarked)
        {
            this.SetGarbageCollectorInformation(isMarked, this.SurvivalCount);
        }

        /// <summary>
        /// Marks the object
        /// </summary>
        public void Mark()
        {
            this.SetMarked(true);
        }

        /// <summary>
        /// Unmarks the object
        /// </summary>
        public void Unmark()
        {
            this.SetMarked(false);
        }

        /// <summary>
        /// Increases the survival count of the object
        /// </summary>
        public void IncreaseSurvivalCount()
        {
            var nextCount = this.SurvivalCount + 1;
            if (nextCount > 127)
            {
                nextCount = 127;
            }

            this.SetGarbageCollectorInformation(this.IsMarked, nextCount);
        }

        /// <summary>
        /// Resets the survival count of the object
        /// </summary>
        public void ResetSurvivalCount()
        {
            this.SetGarbageCollectorInformation(this.IsMarked, 0);
        }

        public override string ToString()
        {
            return $"0x{this.DataPointer.ToString("x8")} ({this.Type})";
        }
    }
}
