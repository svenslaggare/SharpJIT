using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core.Objects;

namespace SharpJIT.Core
{
    /// <summary>
    /// Represents a type provider
    /// </summary>
    public sealed class TypeProvider
    {
        private readonly IDictionary<string, BaseType> types = new Dictionary<string, BaseType>();
        private readonly ClassMetadataProvider classMetadataProvider;

        /// <summary>
        /// Creates a new type provider
        /// </summary>
        /// <param name="classMetadataProvider">The class metadata provider</param>
        public TypeProvider(ClassMetadataProvider classMetadataProvider)
        {
            this.classMetadataProvider = classMetadataProvider;
        }
        
        /// <summary>
        /// Splits the given type name
        /// </summary>
        /// <param name="typeName">The name of the type</param>
        private IList<string> SplitTypeName(string typeName)
        {
            var token = "";
            var typeParts = new List<string>();

            bool isInsideBrackets = false;
            foreach (char c in typeName)
            {
                if (!isInsideBrackets)
                {
                    if (c == '[')
                    {
                        isInsideBrackets = true;
                    }
                }
                else
                {
                    if (c == ']')
                    {
                        isInsideBrackets = false;
                    }
                }

                if (c == '.' && !isInsideBrackets)
                {
                    typeParts.Add(token);
                    token = "";
                }
                else
                {
                    token += c;
                }
            }

            typeParts.Add(token);
            return typeParts;
        }

        /// <summary>
        /// Extracts the element type from the given type part.
        /// </summary>
        /// <param name="typePart">The given type part</param>
        /// <param name="elementPart">The element part</param>
        /// <returns>True if extracted else false</returns>
        private bool ExtractElementType(string typePart, out string elementPart)
        {
           var buffer = "";
            bool foundArrayElement = false;
            bool foundStart = false;
            int bracketCount = 0;

            foreach (char c in typePart)
            {
                if (c == '[')
                {
                    bracketCount++;
                }

                if (c == ']')
                {
                    bracketCount--;
                }

                if (c == ']' && bracketCount == 0)
                {
                    if (foundStart)
                    {
                        foundArrayElement = true;
                    }

                    break;
                }

                buffer += c;

                if (buffer == "Array[" && !foundStart)
                {
                    foundStart = true;
                    buffer = "";
                }
            }

            if (foundArrayElement)
            {
                elementPart = buffer;
                return true;
            }
            else
            {
                elementPart = "";
                return false;
            }
        }

        /// <summary>
        /// Tries to create a type of the given type
        /// </summary>
        /// <param name="name">The name of the type</param>
        private BaseType CreateType(string name)
        {
            //Split the type name
            var typeParts = SplitTypeName(name);
            BaseType type = null;

            if (TypeSystem.FromString(typeParts[0], out var primitiveType))
            {
                type = new PrimitiveType(primitiveType);
            }
            else if (typeParts[0] == "Ref")
            {
                if (ExtractElementType(typeParts[1], out var elementTypeName))
                {
                    var elementType = FindType(elementTypeName);

                    if (elementType != null)
                    {
                        type = new ArrayType(elementType);
                    }
                }
                else if (typeParts[1] == "Null")
                {
                    type = new NullType();
                }
                else
                {
                    if (typeParts.Count >= 2)
                    {
                        var className = "";
                        bool isFirst = true;

                        for (var i = 1; i < typeParts.Count; i++)
                        {
                            if (isFirst)
                            {
                                isFirst = false;
                            }
                            else
                            {
                                className += ".";
                            }

                            className += typeParts[i];
                        }

                        var classMetadata = this.classMetadataProvider.GetMetadata(className);
                        if (classMetadata != null)
                        {
                            type = new ClassType(classMetadata);
                        }
                    }
                }
            }

            if (type != null)
            {
                this.types.Add(name, type);
            }

            return type;
        }

        /// <summary>
        /// Returns the given type.
        /// </summary>
        /// <param name="name">The name of the type</param>
        /// <param name="tryToConstruct">Indicates if to try to construct the type if does not exist.</param>
        /// <returns>The type or null</returns>
        public BaseType FindType(string name, bool tryToConstruct = true)
        {
            if (this.types.TryGetValue(name, out var type))
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
        public BaseType FindPrimitiveType(PrimitiveTypes primitiveType)
        {
            return this.FindType(TypeSystem.ToString(primitiveType));
        }

        /// <summary>
        /// Finds the array type for the given element type
        /// </summary>
        /// <param name="elementType">The element type</param>
        /// <param name="tryToConstruct">Indicates if to try to construct the type if does not exist.</param>
        /// <returns>The type or null</returns>
        public ArrayType FindArrayType(BaseType elementType, bool tryToConstruct = true)
        {
            return this.FindType(TypeSystem.ArrayTypeName(elementType), tryToConstruct) as ArrayType;
        }

        /// <summary>
        /// Finds the given class type
        /// </summary>
        /// <param name="className">The name of the class</param>
        /// <param name="tryToConstruct">Indicates if to try to construct the type if does not exist.</param>
        /// <returns>The type or null</returns>
        public ClassType FindClassType(string className, bool tryToConstruct = true)
        {
            return this.FindType($"Ref.{className}", tryToConstruct) as ClassType;
        }
    }
}
