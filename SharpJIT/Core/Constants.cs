using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core
{
    /// <summary>
    /// Defines constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The size of the object header
        /// </summary>
        public static readonly int ObjectHeaderSize = 1 + 4;

        /// <summary>
        /// The size of the length of the array
        /// </summary>
        public static readonly int ArrayLengthSize = 4;

        /// <summary>
        /// The size of an object reference in bytes
        /// </summary>
        public const int ObjectReferenceSize = 4;

        /// <summary>
        /// The size of an object pointer in bytes
        /// </summary>
        public const int ObjectPointerSize = 8;
    }
}
