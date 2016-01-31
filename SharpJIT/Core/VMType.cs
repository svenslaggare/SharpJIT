using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core
{
    /// <summary>
    /// The primitive types
    /// </summary>
    public enum PrimitiveTypes
    {
        Void,
        Int,
        Float,
        Bool
    }

    /// <summary>
    /// Represents a VM type
    /// </summary>
    public class VMType
    {
        /// <summary>
        /// The name of the type
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new VM type
        /// </summary>
        /// <param name="name">The name of the type</param>
        protected VMType(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name can not be not or empty.", name);
            }

            this.Name = name;
        }

        /// <summary>
        /// Indicates if the current type is of the given primitive type
        /// </summary>
        /// <param name="primitiveType">The primitive type</param>
        public bool IsPrimitiveType(PrimitiveTypes primitiveType)
        {
            return this.Name == TypeSystem.ToString(primitiveType);
        }

        /// <summary>
        /// Returns the string representation of the type
        /// </summary>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Determines if the current obj equals the given object
        /// </summary>
        /// <param name="obj">The other object</param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as VMType;

            if (other == null)
            {
                return false;
            }

            return this.Name == other.Name;
        }

        /// <summary>
        /// Returns the hash code
        /// </summary>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        /// <summary>
        /// Determines if the lhs == rhs
        /// </summary>
        /// <param name="lhs">The left hand side</param>
        /// <param name="rhs">The right hand side</param>
        public static bool operator==(VMType lhs, VMType rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            return lhs.Name == rhs.Name;
        }

        /// <summary>
        /// Determines if the lhs != rhs
        /// </summary>
        /// <param name="lhs">The left hand side</param>
        /// <param name="rhs">The right hand side</param>
        public static bool operator !=(VMType lhs, VMType rhs)
        {
            return !(lhs == rhs);
        }
    }

    /// <summary>
    /// Represents a primitive VM type
    /// </summary>
    public class PrimitiveVMType : VMType
    {
        /// <summary>
        /// Creates a new primitive type
        /// </summary>
        /// <param name="primitiveType">The primitive type</param>
        public PrimitiveVMType(PrimitiveTypes primitiveType)
            : base(TypeSystem.ToString(primitiveType))
        {

        }
    }

    /// <summary>
    /// Contains helper methods for the type system
    /// </summary>
    public static class TypeSystem
    {
        /// <summary>
        /// Returns the name of the given primitive type
        /// </summary>
        /// <param name="primitiveType">The primitive type</param>
        public static string ToString(PrimitiveTypes primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveTypes.Void:
                    return "Void";
                case PrimitiveTypes.Int:
                    return "Int";
                case PrimitiveTypes.Float:
                    return "Float";
                case PrimitiveTypes.Bool:
                    return "Bool";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Returns the primitive type for the given primitive type
        /// </summary>
        /// <param name="typeName">The type name</param>
        /// <param name="primitiveType">The primitive type</param>
        /// <returns>True if primitive type or false</returns>
        public static bool FromString(string typeName, out PrimitiveTypes primitiveType)
        {
            switch (typeName)
            {
                case "Void":
                    primitiveType = PrimitiveTypes.Void;
                    return true;
                case "Int":
                    primitiveType = PrimitiveTypes.Int;
                    return true;
                case "Float":
                    primitiveType = PrimitiveTypes.Float;
                    return true;
                case "Bool":
                    primitiveType = PrimitiveTypes.Bool;
                    return true;
            }

            primitiveType = PrimitiveTypes.Void;
            return false;
        }
    }
}
