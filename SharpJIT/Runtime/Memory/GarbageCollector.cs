using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Core;
using SharpJIT.Runtime.Frame;
using SharpJIT.Runtime.Objects;

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
            this.youngGeneration = new CollectorGeneration(
                virtualMachine.MemoryManager,
                virtualMachine.ManagedObjectReferences,
                4 * 1024 * 1024);
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
            var typeObjectRef = this.virtualMachine.ManagedObjectReferences.GetReference(type);
            NativeHelpers.SetInt(objectPointer, 0, typeObjectRef); //Type
            NativeHelpers.SetByte(objectPointer, Constants.ManagedObjectReferenceSize, 0); //GC info

            //The returned ptr is to the data
            return objectPointer + Constants.ObjectHeaderSize;
        }

        /// <summary>
        /// Deletes the given object
        /// </summary>
        /// <param name="objectReference">A reference to the object</param>
        private void DeleteObject(ObjectReference objectReference)
        {
            NativeHelpers.SetInt(objectReference.FullPointer, 0, objectReference.FullSize); //The amount of data to skip from the start.
            NativeHelpers.SetByte(objectReference.FullPointer, Constants.ManagedObjectReferenceSize, 0xFF); //Indicator for dead object
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
            Console.WriteLine($"Allocated array (length: {length}) with element type of '{elementType}' at 0x{arrayPointer.ToString("x8")}.");
            return arrayPointer;
        }

        /// <summary>
        /// Allocates a new class of the given type
        /// </summary>
        /// <param name="classType">The type of the class</param>
        public IntPtr NewClass(ClassType classType)
        {
            var classPointer = this.AllocateObject(this.youngGeneration, classType, classType.Metadata.Size);
            Console.WriteLine($"Allocated class of type '{classType.ClassName}' at 0x{classPointer.ToString("x8")}.");
            return classPointer;
        }

        /// <summary>
        /// Gets a reference to the given object 
        /// </summary>
        /// <param name="objectPointer">The pointer to the object</param>
        public ObjectReference GetObjectReference(IntPtr objectPointer)
        {
            return new ObjectReference(this.virtualMachine.ManagedObjectReferences, objectPointer);
        }

        /// <summary>
        /// Marks the given object
        /// </summary>
        /// <param name="generation">The generation</param>
        /// <param name="objectReference">A reference to the object</param>
        private void MarkObject(CollectorGeneration generation, ObjectReference objectReference)
        {
            if (!objectReference.IsMarked)
            {
                objectReference.Mark();

                if (objectReference.Type.IsArray)
                {
                    //Mark ref elements
                    var arrayType = (ArrayType)objectReference.Type;

                    if (arrayType.ElementType.IsReference)
                    {
                        var arrayReference = new ArrayReference(objectReference);

                        for (int i = 0; i < arrayReference.Length; i++)
                        {
                            var elementPtr = arrayReference.GetElement(i);
                            this.MarkValue(generation, NativeHelpers.ReadLong(elementPtr), arrayType.ElementType);
                        }
                    }
                }
                else if (objectReference.Type.IsClass)
                {
                    //Mark ref fields
                    var classType = (ClassType)objectReference.Type;
                    foreach (var field in classType.Metadata.Fields)
                    {
                        if (field.Type.IsReference)
                        {
                            var fieldValue = NativeHelpers.ReadLong(objectReference.DataPointer + field.LayoutOffset);
                            this.MarkValue(generation, fieldValue, field.Type);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Marks the given value
        /// </summary>
        /// <param name="generation">The generation</param>
        /// <param name="value">The value</param>
        /// <param name="type">The type of the value</param>
        private void MarkValue(CollectorGeneration generation, long value, BaseType type)
        {
            if (type.IsReference)
            {
                //Don't mark nulls
                if (value == 0)
                {
                    return;
                }

                this.MarkObject(generation, this.GetObjectReference(new IntPtr(value)));
            }
        }

        /// <summary>
        /// Marks all the objects starting at the given frame
        /// </summary>
        /// <param name="generation">The generation</param>
        /// <param name="stackFrame">The stack frame</param>
        private void MarkAllObjects(CollectorGeneration generation, StackFrame stackFrame)
        {
            var stalkWalker = new StackWalker(this.virtualMachine.CallStack);
            stalkWalker.VisitObjectReferences(
                stackFrame,
                frameEntry =>
                {
                    var objectPointer = new IntPtr(frameEntry.Value);
                    this.MarkObject(generation, this.GetObjectReference(objectPointer));
                },
                frame =>
                {
                    Console.WriteLine($"{frame.Function.Definition.Name} ({frame.InstructionIndex})");
                    RuntimeInterface.PrintStackFrame(frame, false);
                });
        }

        /// <summary>
        /// Sweeps the objects in the given generation
        /// </summary>
        /// <param name="generation">The generation</param>
        private void SweepObjects(CollectorGeneration generation)
        {
            generation.Heap.VisitObjects(objectReference =>
            {
                if (objectReference.IsMarked)
                {
                    objectReference.Unmark();
                    objectReference.IncreaseSurvivalCount();
                }
                else
                {
                    Console.WriteLine($"Deleted object {objectReference}");
                    this.DeleteObject(objectReference);
                }
            });
        }

        /// <summary>
        /// Starts a garbage collection at the given stack frame
        /// </summary>
        /// <param name="stackFrame">The current stack frame</param>
        public void Collect(StackFrame stackFrame)
        {
            Console.WriteLine("Alive objects:");
            youngGeneration.Heap.VisitObjects(objectReference =>
            {
                Console.WriteLine(objectReference);
            });
            Console.WriteLine("");

            this.MarkAllObjects(this.youngGeneration, stackFrame);
            this.SweepObjects(this.youngGeneration);
        }
    }
}
