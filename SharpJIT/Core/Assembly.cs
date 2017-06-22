using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// The functions in the assembly
        /// </summary>
        public IReadOnlyList<ManagedFunction> Functions { get; }

        /// <summary>
        /// Creates a new assembly
        /// </summary>
        /// <param name="name">The name of the assembly</param>
        /// <param name="functions">The functions</param>
        public Assembly(string name, IList<ManagedFunction> functions)
        {
            this.Name = name;
            this.Functions = new ReadOnlyCollection<ManagedFunction>(new List<ManagedFunction>(functions));
        }

        /// <summary>
        /// Creates a new assembly
        /// </summary>
        /// <param name="name">The name of the assembly</param>
        /// <param name="functions">The functions</param>
        public Assembly(string name, params ManagedFunction[] functions)
        {
            this.Name = name;
            this.Functions = new ReadOnlyCollection<ManagedFunction>(functions.ToList());
        }

        /// <summary>
        /// Creates an assembly that just has one function
        /// </summary>
        /// <param name="function">The function</param>
        /// <param name="name">The name of the assembly</param>
        public static Assembly SingleFunction(ManagedFunction function, string name = "")
        {
            return new Assembly(name, new List<ManagedFunction>() { function });
        }
    }
}
