using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core.Objects;

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
    /// Represents a type in the VM
    /// </summary>
    public abstract class BaseType
    {
        /// <summary>
        /// The name of the type
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new VM type
        /// </summary>
        /// <param name="name">The name of the type</param>
        public BaseType(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name cannot be null or empty.", name);
            }

            this.Name = name;
        }

        /// <summary>
        /// Returns the string representation of the type
        /// </summary>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Compares if the given types are equal
        /// </summary>
        /// <param name="first">The first type</param>
        /// <param name="second">The second type</param>
        private static bool Equals(BaseType first, BaseType second)
        {
            return
                first.Name == second.Name
                || (first.IsReference() && TypeSystem.IsNullType(second))
                || (second.IsReference() && TypeSystem.IsNullType(first));
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

            if (obj == null)
            {
                return false;
            }

            var other = obj as BaseType;
            return Equals(this, other);
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
        public static bool operator ==(BaseType lhs, BaseType rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
            {
                return false;
            }

            return Equals(lhs, rhs);
        }

        /// <summary>
        /// Determines if the lhs != rhs
        /// </summary>
        /// <param name="lhs">The left hand side</param>
        /// <param name="rhs">The right hand side</param>
        public static bool operator !=(BaseType lhs, BaseType rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Indicates if the current type is of the given primitive type
        /// </summary><
        /// <param name="primitiveType">The primitive type</param>
        public bool IsPrimitiveType(PrimitiveTypes primitiveType)
        {
            if (TypeSystem.FromString(this.Name, out var outType))
            {
                return outType == primitiveType;
            }

            return false;
        }

        /// <summary>
        /// Indicates if the current type is a reference type
        /// </summary>
        public abstract bool IsReference();

        /// <summary>
        /// Indicates if the current type is an array type
        /// </summary>
        public abstract bool IsArray();

        /// <summary>
        /// Indicates if the current type is a class type
        /// </summary>
        public abstract bool IsClass();
    }

    /// <summary>
    /// Represents a primitive type
    /// </summary>
    public sealed class PrimitiveType : BaseType
    {
        /// <summary>
        /// Creates a new primitive type
        /// </summary>
        /// <param name="primitiveType">The primitive type</param>
        public PrimitiveType(PrimitiveTypes primitiveType)
            : base(TypeSystem.ToString(primitiveType))
        {

        }

        public override bool IsArray()
        {
            return false;
        }

        public override bool IsClass()
        {
            return false;
        }

        public override bool IsReference()
        {
            return false;
        }
    }

    /// <summary>
    /// Represents the null type
    /// </summary>
    public sealed class NullType : BaseType
    {
        /// <summary>
        /// Creates a new null type
        /// </summary>
        public NullType()
            : base("Ref.Null")
        {

        }

        public override bool IsReference()
        {
            return true;
        }

        public override bool IsArray()
        {
            return false;
        }

        public override bool IsClass()
        {
            return false;
        }
    }

    /// <summary>
    /// Represents an array type
    /// </summary>
    public sealed class ArrayType : BaseType
    {
        /// <summary>
        /// The element type.
        /// </summary>
        public BaseType ElementType { get; }

        /// <summary>
        /// Creates a new array type
        /// </summary>
        /// <param name="elementType">The type of the element</param>
        public ArrayType(BaseType elementType)
            : base(TypeSystem.ArrayTypeName(elementType))
        {
            this.ElementType = elementType;
        }

        public override bool IsReference()
        {
            return true;
        }

        public override bool IsArray()
        {
            return true;
        }

        public override bool IsClass()
        {
            return false;
        }
    }

    /// <summary>
    /// Represents a class type
    /// </summary>
    public sealed class ClassType : BaseType
    {
        /// <summary>
        /// Returns the metadata for the class
        /// </summary>
        public ClassMetadata Metadata { get; }

        public ClassType(ClassMetadata metadata)
            : base("Ref." + metadata.Name)
        {
            this.Metadata = metadata;
        }

        /// <summary>
        /// Returns the name of the class
        /// </summary>
        public string ClassName => this.Metadata.Name;

        public override bool IsReference()
        {
            return true;
        }

        public override bool IsArray()
        {
            return false;
        }

        public override bool IsClass()
        {
            return true;
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

        /// <summary>
        /// Returns the size of the given primitive type in bytes
        /// </summary>
        /// <param name="primitiveType">The primitive type</param>
        public static int SizeOf(PrimitiveTypes primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveTypes.Void:
                    return 0;
                case PrimitiveTypes.Int:
                    return 4;
                case PrimitiveTypes.Float:
                    return 4;
                case PrimitiveTypes.Bool:
                    return 1;
            }

            return 0;
        }

        /// <summary>
        /// Returns the size of the given type in bytes
        /// </summary>
        /// <param name="type">The type</param>
        public static int SizeOf(BaseType type)
        {
            if (FromString(type.Name, out var primitiveType))
            {
                return SizeOf(primitiveType);
            }
            else
            {
                return Constants.ObjectPointerSize;
            }
        }

        /// <summary>
        /// Returns an array type name
        /// </summary>
        /// <param name="elementType">The type of the element.</param>
        public static string ArrayTypeName(BaseType elementType)
        {
            return $"Ref.Array[{elementType.ToString()}]";
        }

        /// <summary>
        /// Returns the type name for the given class
        /// </summary>
        /// <param name="className">The name of the class</param>
        public static string ClassTypeName(string className)
        {
            return "Ref." + className;
        }

        /// <summary>
        /// Checks if the given type is the null type
        /// </summary>
        /// <param name="type">The type</param>
        public static bool IsNullType(BaseType type)
        {
            return type is NullType;
        }

        /// <summary>
        /// Extracts the class and field name from the given string
        /// </summary>
        /// <param name="text">The string</param>
        /// <param name="className">The class name</param>
        /// <param name="fieldName">The field name</param>
        /// <returns>True if success</returns>
        public static bool ExtractClassAndFieldName(string text, out string className, out string fieldName)
        {
            var fieldSeparationPosition = text.IndexOf("::");

            if (fieldSeparationPosition != -1) {
                className = text.Substring(0, fieldSeparationPosition);
                fieldName = text.Substring(fieldSeparationPosition + 2);
                return true;
            } else {
                className = "";
                fieldName = "";
                return false;
            }
        }
    }
}
