using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Core;

namespace SharpJIT.Runtime.Memory
{
    /// <summary>
    /// Represents a garbage collector.
    /// </summary>
    public sealed class GarbageCollector
    {
        private readonly VirtualMachine virtualMachine;
        private readonly CollectorGeneration youngGeneration;

        /// <summary>
        /// Creates a new garbage collector
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public GarbageCollector(VirtualMachine virtualMachine)
        {
            this.virtualMachine = virtualMachine;
            this.youngGeneration = new CollectorGeneration(virtualMachine.MemoryManager, 4 * 1024 * 1024);
        }

        /// <summary>
        /// Allocates an object of the given size
        /// </summary>
        /// <param name="generation">The generation to allocate in</param>
        /// <param name="type">The type of the object</param>
        /// <param name="size">The size of the object</param>
        private IntPtr AllocateObject(CollectorGeneration generation, BaseType type, int size)
        {
            var fullSize = Constants.ObjectHeaderSize + size;
            var objectPointer = generation.Allocate(fullSize);

            NativeHelpers.SetBlock(objectPointer, fullSize, 0);

            //Set the header
            var typeObjectRef = this.virtualMachine.ObjectReferences.GetReference(type);
            NativeHelpers.SetInt(objectPointer, 0, typeObjectRef); //Type
            NativeHelpers.SetByte(objectPointer, Constants.ObjectReferenceSize, 0); //GC info

            //The returned ptr is to the data
            return objectPointer + Constants.ObjectHeaderSize;
        }

        /// <summary>
        /// Allocates a new array of the given type and length
        /// </summary>
        /// <param name="arrayType">The type of the array</param>
        /// <param name="length">The length</param>
        public IntPtr NewArray(ArrayType arrayType, int length)
        {
            var elementType = arrayType.ElementType;
            var elementSize = TypeSystem.SizeOf(elementType);

            var objectSize = Constants.ArrayLengthSize + (length * elementSize);
            var arrayPointer = this.AllocateObject(this.youngGeneration, arrayType, objectSize);

            //Set the length of the array
            NativeHelpers.SetInt(arrayPointer, 0, length);
            return arrayPointer;
        }

        /// <summary>
        /// Allocates a new class of the given type
        /// </summary>
        /// <param name="classType">The type of the class</param>
        public IntPtr NewClass(ClassType classType)
        {
            var classPointer = this.AllocateObject(this.youngGeneration, classType, classType.Metadata.Size);
            return classPointer;
        }
    }
}
