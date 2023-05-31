using UnityEngine;

namespace Unisave.Heapstore
{
    /// <summary>
    /// Reference to a database collection
    /// </summary>
    public class CollectionReference
    {
        /// <summary>
        /// Name of the referenced collection
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// To whom facet calls will be attached
        /// </summary>
        public MonoBehaviour Caller { get; }

        public CollectionReference(string name)
        {
            Name = name;
        }
        
        public CollectionReference(string name, MonoBehaviour caller)
        {
            Name = name;
            Caller = caller;
        }
        
        
        //////////////////////////
        // Document referencing //
        //////////////////////////
        
        public DocumentReference Document(string key)
            => new DocumentReference(Name, key, Caller);
        
        
        //////////////
        // Querying //
        //////////////
    }
}