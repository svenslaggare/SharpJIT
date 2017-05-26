using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core.Objects
{
    /// <summary>
    /// Defines the metadata for a class
    /// </summary>
    public class ClassMetadata
    {
        /// <summary>
        /// The name of the class
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The size of the class in bytes
        /// </summary>
        public int Size { get; private set; }

        private readonly IList<FieldDefinition> fieldDefinitions = new List<FieldDefinition>();
        private readonly IDictionary<string, Field> fields = new Dictionary<string, Field>();

        /// <summary>
        /// Creates a metadata for the given class
        /// </summary>
        /// <param name="name">The name of the class</param>
        public ClassMetadata(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Returns the fields
        /// </summary>
        /// <remarks>
        ///     If <see cref="CreateFields"/> has not been called, this returns zero, even if fields has been defined.
        /// </remarks>
        public IEnumerable<Field> Fields => this.fields.Values;

        /// <summary>
        /// Defines the given field
        /// </summary>
        /// <param name="field">The field</param>
        public void DefineField(FieldDefinition field)
        {
            this.fieldDefinitions.Add(field);
        }

        /// <summary>
        /// Inserts the given field
        /// </summary>
        /// <param name="field">The field</param>
        private void InsertField(Field field)
        {
            if (this.fields.ContainsKey(field.Name))
            {
                throw new ArgumentException($"The field '{field.Name}' already is defined in the class '{this.Name}'");
            }

            this.fields.Add(field.Name, field);
        }

        /// <summary>
        /// Creates the defined fields
        /// </summary>
        public void CreateFields()
        {
            if (this.fields.Count == 0)
            {
                foreach (var field in this.fieldDefinitions)
                {
                    this.InsertField(new Field(field.Name, field.Type, field.AccessModifier, this.Size));
                    this.Size += TypeSystem.SizeOf(field.Type);
                }
            }
        }
    }
}
