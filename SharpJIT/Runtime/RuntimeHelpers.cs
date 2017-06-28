using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core;

namespace SharpJIT.Runtime
{
    /// <summary>
    /// Contains helper methods for the runtime
    /// </summary>
    public static class RuntimeHelpers
    {
        /// <summary>
        /// Converts the given value into a string
        /// </summary>
        /// <param name="value">A raw value</param>
        /// <param name="type">The type of the value</param>
        public static string ValueToString(long value, BaseType type)
        {
            switch (type)
            {
                case PrimitiveType primitiveType when (primitiveType.IsPrimitiveType(PrimitiveTypes.Int)):
                    return value.ToString();
                case PrimitiveType primitiveType when (primitiveType.IsPrimitiveType(PrimitiveTypes.Float)):
                    var floatValue = BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
                    return floatValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case BaseType referenceType when (referenceType.IsReference):
                    if (value == 0)
                    {
                        return "nullref";
                    }
                    else
                    {
                        return "0x" + value.ToString("x8");
                    }
                default:
                    return value.ToString();
            }
        }
    }
}
