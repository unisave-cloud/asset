using UnityEngine;

namespace Unisave.Heapstore
{
    /// <summary>
    /// Reference to a database collection
    /// </summary>
    public class CollectionReference : Query
    {
        protected string CollectionName => request.collection;
        
        public CollectionReference(string collectionName)
            : base(collectionName) { }
        
        public CollectionReference(string collectionName, MonoBehaviour caller)
            : base(collectionName, caller) { }
        
        
        //////////////////////////
        // Document referencing //
        //////////////////////////
        
        public DocumentReference Document(string key)
            => new DocumentReference(CollectionName, key, caller);
    }
}