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
    }
}
