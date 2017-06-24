using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core.Objects;

namespace SharpJIT.Core
{
    /// <summary>
    /// Represents an assembly
    /// </summary>
    public sealed class Assembly
    {
        /// <summary>
        /// The name of the assembly
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The classes in the assembly
        /// </summary>
        public IReadOnlyList<ClassMetadata> Classes { get; }

        /// <summary>
        /// The functions in the assembly
        /// </summary>
        public IReadOnlyList<ManagedFunction> Functions { get; }

        /// <summary>
        /// Creates a new assembly
        /// </summary>
        /// <param name="name">The name of the assembly</param>
        /// <param name="classes">The classes</param>
        /// <param name="functions">The functions</param>
        public Assembly(string name, IList<ClassMetadata> classes, IList<ManagedFunction> functions)
        {
            this.Name = name;
            this.Classes = new ReadOnlyCollection<ClassMetadata>(new List<ClassMetadata>(classes));
            this.Functions = new ReadOnlyCollection<ManagedFunction>(new List<ManagedFunction>(functions));
        }
    }
}
