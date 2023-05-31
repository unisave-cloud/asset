using LightJson;
using Unisave.Facets;
using Unisave.Heapstore.Backend;
using Unisave.Serialization;
using UnityEngine;

namespace Unisave.Heapstore
{
    /// <summary>
    /// Reference to a specific database document
    /// (which may or may not exist)
    /// </summary>
    public class DocumentReference
    {
        /// <summary>
        /// Name of the referenced collection
        /// </summary>
        public string Collection { get; }
        
        /// <summary>
        /// Key of the referenced document
        /// </summary>
        public string DocumentKey { get; }

        /// <summary>
        /// Id of the referenced document
        /// </summary>
        public string DocumentId => DocumentKey == null
            ? null : Collection + "/" + DocumentKey;
        
        /// <summary>
        /// To whom facet calls will be attached
        /// </summary>
        public MonoBehaviour Caller { get; }

        public DocumentReference(string collection, string key)
        {
            Collection = collection;
            DocumentKey = key;
        }
        
        public DocumentReference(string collection, string key, MonoBehaviour caller)
        {
            Collection = collection;
            DocumentKey = key;
            Caller = caller;
        }
        
        
        /////////////
        // Actions //
        /////////////
        
        public FacetCall Set<T>(T value)
        {
            string id = DocumentId;
            JsonObject json = Serializer.ToJson<T>(value);
            // TODO: specify the serialization context

            return Caller.CallFacet((HeapstoreFacet f) => f.SetDocument(id, json));
        }
    }
}