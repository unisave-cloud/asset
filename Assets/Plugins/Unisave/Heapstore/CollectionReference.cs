using UnityEngine;

namespace Unisave.Heapstore
{
    /// <summary>
    /// References a database collection and allows you
    /// to reference documents or build document queries
    /// </summary>
    public class CollectionReference : Query
    {
        /// <summary>
        /// Name of the referenced collection
        /// </summary>
        private string CollectionName => request.collection;
        
        public CollectionReference(string collectionName)
            : base(collectionName) { }
        
        public CollectionReference(string collectionName, MonoBehaviour caller)
            : base(collectionName, caller) { }
        
        /// <summary>
        /// Creates a reference to a specific document so that you can
        /// then perform operations with it
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DocumentReference Document(string key)
        {
            return new DocumentReference(CollectionName, key, caller);
        }
    }
}