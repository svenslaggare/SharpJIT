using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Runtime.Memory
{
    /// <summary>
    /// Handles references to object passed between the VM and managed code
    /// </summary>
    public sealed class ObjectReferences
    {
        private readonly IDictionary<int, object> idToObjects = new Dictionary<int, object>();
        private readonly IDictionary<object, int> objectToIds = new Dictionary<object, int>();

        private int lastObjectId = 0;

        /// <summary>
        /// Returns a reference to the given object
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>The reference id</returns>
        public int GetReference(object obj)
        {
            if (!this.objectToIds.TryGetValue(obj, out var objectId))
            {
                objectId = this.lastObjectId++;
                this.objectToIds.Add(obj, objectId);
                this.idToObjects.Add(objectId, obj);
            }

            return objectId;
        }

        /// <summary>
        /// Returns the object with the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The object</returns>
        /// <exception cref="KeyNotFoundException">If not found.</exception>
        public object GetObject(int id)
        {
            if (this.idToObjects.ContainsKey(id))
            {
                return this.idToObjects[id];
            }

            throw new KeyNotFoundException("No object exists with the given id.");
        }

        /// <summary>
        /// Returns the object with the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The object</returns>
        /// <exception cref="KeyNotFoundException">If not found.</exception>
        /// <typeparam name="T">The type of the object</typeparam>
        public T GetObject<T>(int id)
        {
            return (T)this.GetObject(id);
        }
    }
}
