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
    public class Assembly
    {
        /// <summary>
        /// The functions in the assembly
        /// </summary>
        public IReadOnlyList<Function> Functions { get; }

        /// <summary>
        /// Creates a new assembly
        /// </summary>
        /// <param name="functions">The functions</param>
        public Assembly(IList<Function> functions)
        {
            this.Functions = new ReadOnlyCollection<Function>(functions);
        }

        /// <summary>
        /// Creates a new assembly
        /// </summary>
        /// <param name="functions">The functions</param>
        public Assembly(params Function[] functions)
        {
            this.Functions = new ReadOnlyCollection<Function>(functions.ToList());
        }

        /// <summary>
        /// Creates an assembly that just has one function
        /// </summary>
        /// <param name="function">The function</param>
        public static Assembly SingleFunction(Function function)
        {
            return new Assembly(new List<Function>() { function });
        }
    }
}
