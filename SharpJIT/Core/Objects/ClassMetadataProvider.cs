using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core.Objects
{
    /// <summary>
    /// Represents a provider for class metadata
    /// </summary>
    public sealed class ClassMetadataProvider
    {
        private readonly IDictionary<string, ClassMetadata> classesMetadata = new Dictionary<string, ClassMetadata>();

        /// <summary>
        /// Adds the given class to list of defined classes
        /// </summary>
        /// <param name="classMetadata">The class to add</param>
        public void Add(ClassMetadata classMetadata)
        {
            if (!this.classesMetadata.ContainsKey(classMetadata.Name))
            {
                this.classesMetadata.Add(classMetadata.Name, classMetadata);
            }
        }

        /// <summary>
        /// Returns the metadata for the given class
        /// </summary>
        /// <param name="className">The name of the class</param>
        /// <returns>The metadata or null if not defined</returns>
        public ClassMetadata GetMetadata(string className)
        {
            if (this.classesMetadata.TryGetValue(className, out var classMetadata))
            {
                return classMetadata;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Indicates if the given class is defined
        /// </summary>
        /// <param name="className">The name of the class</param>
        public bool IsDefined(string className)
        {
            return this.classesMetadata.ContainsKey(className);
        }
    }
}
