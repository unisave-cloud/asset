using LightJson;
using Unisave.Arango;
using Unisave.Serialization;
using Unisave.Serialization.Context;

namespace Unisave.Heapstore
{
    public class Document
    {
        public string Collection => documentId.Collection;
        public string Key => documentId.Key;
        public string Id => documentId.Id;

        private DocumentId documentId;
        
        public JsonObject Data { get; }

        public string Revision { get; }

        public Document(JsonObject document)
        {
            documentId = DocumentId.Parse(document["_id"]);
            Revision = document["_rev"];

            document.Remove("_key");
            document.Remove("_id");
            document.Remove("_rev");
            
            Data = document;
        }

        public T As<T>()
        {
            return Serializer.FromJson<T>(
                Data,
                DeserializationContext.ClientToClient
            );
        }
    }
}