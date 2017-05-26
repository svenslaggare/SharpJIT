using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core.Objects
{
    /// <summary>
    /// The access modifiers
    /// </summary>
    public enum AccessModifier
    {
        Private,
        Public
    }

    /// <summary>
    /// Represents a field in a class
    /// </summary>
    public sealed class Field
    {
        /// <summary>
        /// The name of the field
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the field
        /// </summary>
        public BaseType Type { get; }

        /// <summary>
        /// The access modifier
        /// </summary>
        public AccessModifier AccessModifier { get; }

        /// <summary>
        /// The offset for the field in the memory layout of the class
        /// </summary>
        public int LayoutOffset { get; }

        /// <summary>
        /// Creates a new field
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="type">The type of the field</param>
        /// <param name="accessModifier">The access modifier</param>
        /// <param name="layoutOffset">The offset for the field in the memory layout of the class</param>
        public Field(string name, BaseType type, AccessModifier accessModifier, int layoutOffset)
        {
            this.Name = name;
            this.Type = type;
            this.AccessModifier = AccessModifier;
            this.LayoutOffset = layoutOffset;
        }
    }

    /// <summary>
    /// Represents the definition of a field
    /// </summary>
    public sealed class FieldDefinition
    {
        /// <summary>
        /// The name of the field
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the field
        /// </summary>
        public BaseType Type { get; }

        /// <summary>
        /// The access modifier
        /// </summary>
        public AccessModifier AccessModifier { get; }

        /// <summary>
        /// Creates a new field definition
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="type">The type of the field</param>
        /// <param name="accessModifier">The access modifier</param>
        /// <param name="layoutOffset">The offset for the field in the memory layout of the class</param>
        public FieldDefinition(string name, BaseType type, AccessModifier accessModifier)
        {
            this.Name = name;
            this.Type = type;
            this.AccessModifier = AccessModifier;
        }
    }
}
