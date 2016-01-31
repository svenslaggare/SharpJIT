using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core
{
    /// <summary>
    /// Represents a type provider
    /// </summary>
    public class TypeProvider
    {
        private readonly IDictionary<string, VMType> types = new Dictionary<string, VMType>();

        /// <summary>
        /// Creates a new type provider
        /// </summary>
        public TypeProvider()
        {

        }

        /// <summary>
        /// Tries to create a type of the given type
        /// </summary>
        /// <param name="name">The name of the type</param>
        private VMType CreateType(string name)
        {
            PrimitiveTypes primitiveType;
            if (TypeSystem.FromString(name, out primitiveType))
            {
                return new PrimitiveVMType(primitiveType);
            }

            return null;
        }

        /// <summary>
        /// Returns the given type.
        /// </summary>
        /// <param name="name">The name of the type</param>
        /// <param name="tryToConstruct">Indicates if to try to construct the type if does not exist.</param>
        /// <returns>The type or null</returns>
        public VMType GetType(string name, bool tryToConstruct = true)
        {
            VMType type;
            if (this.types.TryGetValue(name, out type))
            {
                return type;
            }
            else
            {
                if (tryToConstruct)
                {
                    return this.CreateType(name);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the given primitive type
        /// </summary>
        /// <param name="primitiveType">The primitive type</param>
        public VMType GetPrimitiveType(PrimitiveTypes primitiveType)
        {
            return this.GetType(TypeSystem.ToString(primitiveType));
        }
    }
}
