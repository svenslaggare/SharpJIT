using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core
{
    /// <summary>
    /// Represents the function binder
    /// </summary>
    public class Binder
    {
        private readonly IDictionary<string, FunctionDefinition> functions;

        /// <summary>
        /// Creates a new binder
        /// </summary>
        public Binder()
        {
            this.functions = new Dictionary<string, FunctionDefinition>();
        }

        /// <summary>
        /// Returns the signature for the given function
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="parameters">The parameter types</param>
        public string FunctionSignature(string name, IEnumerable<VMType> parameters)
        {
            return $"{name}({string.Join(" ", parameters)})";
        }

        /// <summary>
        /// Returns the signature for the given function
        /// </summary>
        /// <param name="function"></param>
        public string FunctionSignature(FunctionDefinition function)
        {
            return this.FunctionSignature(function.Name, function.Parameters);
        }

        /// <summary>
        /// Defines the given function
        /// </summary>
        /// <param name="function">The function to define</param>
        /// <returns>True if defined else false</returns>
        public bool Define(FunctionDefinition function)
        {
            var signature = this.FunctionSignature(function);

            if (!this.functions.ContainsKey(signature))
            {
                this.functions.Add(signature, function);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the function with the given signature
        /// </summary>
        /// <param name="signature">The signature</param>
        /// <returns>The function or null</returns>
        public FunctionDefinition GetFunction(string signature)
        {
            FunctionDefinition func;
            if (this.functions.TryGetValue(signature, out func))
            {
                return func;
            }
            else
            {
                return null;
            }
        }
    }
}
