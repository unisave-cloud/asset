using System.Threading.Tasks;
using LightJson;
using Unisave.Facets;
using Unisave.Heapstore.Backend;
using Unisave.Serialization;
using Unisave.Serialization.Context;
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
        
        
        /////////////////////////
        // Document operations //
        /////////////////////////

        public UnisaveOperation<Document> Get()
        {
            return new UnisaveOperation<Document>(Caller, GetAsync());
        }

        private async Task<Document> GetAsync()
        {
            var id = Arango.DocumentId.Parse(DocumentId);
            JsonObject fetchedJson = await Caller.CallFacet(
                (HeapstoreFacet f) => f.GetDocument(id)
            );
            
            return fetchedJson == null ? null : new Document(fetchedJson);
        }
        
        public UnisaveOperation<Document> Set<T>(T value)
        {
            return new UnisaveOperation<Document>(Caller, SetAsync(value));
        }
        
        private async Task<Document> SetAsync<T>(T value)
        {
            JsonObject jsonToWrite = Serializer.ToJson<T>(
                value,
                SerializationContext.ClientToClient
            );

            var id = Arango.DocumentId.Parse(DocumentId);
            JsonObject writtenJson = await Caller.CallFacet(
                (HeapstoreFacet f) => f.SetDocument(id, jsonToWrite)
            );

            return new Document(writtenJson);
        }
    }
}