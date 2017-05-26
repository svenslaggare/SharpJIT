using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core
{
    /// <summary>
    /// Represents a definition for a function
    /// </summary>
    public class FunctionDefinition
    {
        /// <summary>
        /// The name of the function
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The parameters
        /// </summary>
        public IReadOnlyList<BaseType> Parameters { get; }

        /// <summary>
        /// The return type
        /// </summary>
        public BaseType ReturnType { get; }

        /// <summary>
        /// Indicates if the current function is managed
        /// </summary>
        public bool IsManaged { get; }

        /// <summary>
        /// Returns the entry point of the function
        /// </summary>
        public IntPtr EntryPoint { get; private set; }

        /// <summary>
        /// Creates a new function definition for a managed function
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="returnType">The return type</param>
        public FunctionDefinition(string name, IList<BaseType> parameters, BaseType returnType)
        {
            this.Name = name;
            this.Parameters = new ReadOnlyCollection<BaseType>(parameters);
            this.ReturnType = returnType;
            this.IsManaged = true;
        }

        /// <summary>
        /// Creates a new function definition for an unmanaged function
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="returnType">The return type</param>
        /// <param name="entryPoint">The entry point</param>
        public FunctionDefinition(string name, IList<BaseType> parameters, BaseType returnType, IntPtr entryPoint)
        {
            this.Name = name;
            this.Parameters = new ReadOnlyCollection<BaseType>(parameters);
            this.ReturnType = returnType;
            this.IsManaged = false;
            this.EntryPoint = entryPoint;
        }

        /// <summary>
        /// Creates a new external function
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate</typeparam>
        /// <param name="name">The name of the function</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="returnType">The return type</param>
        /// <param name="funcDelegate">The delegate for the function</param>
        public static FunctionDefinition NewExternal<TDelegate>(string name, IList<BaseType> parameters, BaseType returnType, TDelegate funcDelegate)
        {
            return new FunctionDefinition(
                name,
                parameters,
                returnType,
                Marshal.GetFunctionPointerForDelegate(funcDelegate));
        }

        /// <summary>
        /// Sets the entry point for managed functions
        /// </summary>
        /// <param name="entryPoint">The entry point</param>
        public void SetEntryPoint(IntPtr entryPoint)
        {
            if (this.IsManaged)
            {
                this.EntryPoint = entryPoint;
            }
            else
            {
                throw new InvalidOperationException("The current function is not a managed function.");
            }
        }

        public override string ToString()
        {
            return $"{this.Name}({string.Join(" ", this.Parameters)}) {this.ReturnType}";
        }
    }
}
