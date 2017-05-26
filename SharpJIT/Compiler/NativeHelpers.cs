using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Compiler
{
	/// <summary>
	/// Contains native helper methods
	/// </summary>
	public static class NativeHelpers
	{
		/// <summary>
		/// Copies to source list to the destination
		/// </summary>
		/// <param name="destination">The destination</param>
		/// <param name="source">The source</param>
		public static unsafe void CopyTo(IntPtr destination, IList<byte> source)
		{
			var memPtr = (byte*)destination.ToPointer();
            for (int i = 0; i < source.Count; i++)
			{
				memPtr[i] = source[i];
			}
		}

        /// <summary>
        /// Sets the int in the given byte vector starting at the given index
        /// </summary>
        /// <param name="source">The source</param>
        /// <param name="startIndex">The start index</param>
        /// <param name="value">The value</param>
        public static void SetInt(IList<byte> source, int startIndex, int value)
        {
            int index = startIndex;
            foreach (var component in BitConverter.GetBytes(value))
            {
                source[index] = component;
                index++;
            }
        }

        /// <summary>
        /// Sets the long in the given byte vector starting at the given index
        /// </summary>
        /// <param name="source">The source</param>
        /// <param name="startIndex">The start index</param>
        /// <param name="value">The value</param>
        public static void SetLong(IList<byte> source, int startIndex, long value)
        {
            int index = startIndex;
            foreach (var component in BitConverter.GetBytes(value))
            {
                source[index] = component;
                index++;
            }
        }


        /// <summary>
        /// Sets the given 8-bits integer
        /// </summary>
        /// <param name="start">The start of the memory block</param>
        /// <param name="offset">The offset (in bytes)</param>
        /// <param name="value">The value</param>
        public static unsafe void SetByte(IntPtr start, int offset, byte value)
        {
            var memPtr = (byte*)start.ToPointer();
            memPtr[offset] = value;
        }

        /// <summary>
        /// Sets the given 32-bits integer
        /// </summary>
        /// <param name="start">The start of the memory block</param>
        /// <param name="offset">The offset (in bytes)</param>
        /// <param name="value">The value</param>
        public static unsafe void SetInt(IntPtr start, int offset, int value)
        {
            var memPtr = (byte*)start.ToPointer();
            long index = offset;
            foreach (var component in BitConverter.GetBytes(value))
            {
                memPtr[index] = component;
                index++;
            }
        }

        /// <summary>
        /// Sets the given 64-bits integer
        /// </summary>
        /// <param name="start">The start of the memory block</param>
        /// <param name="offset">The offset (in bytes)</param>
        /// <param name="value">The value</param>
        public static unsafe void SetLong(IntPtr start, int offset, long value)
        {
            var memPtr = (byte*)start.ToPointer();
            long index = offset;
            foreach (var component in BitConverter.GetBytes(value))
            {
                memPtr[index] = component;
                index++;
            }
        }

        /// <summary>
        /// Sets the value for the given memory block
        /// </summary>
        /// <param name="start">The start of the block</param>
        /// <param name="size">The size of the block</param>
        /// <param name="value">The value</param>
        public static unsafe void SetBlock(IntPtr start, int size, byte value)
        {
            var memPtr = (byte*)start.ToPointer();
            for (long i = 0; i < size; i++)
            {
                memPtr[i] = value;
            }
        }
    }
}
