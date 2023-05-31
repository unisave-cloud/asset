using LightJson;
using Unisave.Arango;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Facets;
using UnityEngine;

namespace Unisave.Heapstore.Backend
{
    public class HeapstoreFacet : Facet
    {
        public void SetDocument(string id, JsonObject document)
        {
            try
            {
                TrySetDocument(id, document);
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                var i = DocumentId.Parse(id);
                CreateCollection(i.Collection);
                TrySetDocument(id, document);
            }
        }

        private void CreateCollection(string name)
        {
            var arango = (ArangoConnection) Facade.App.Resolve<IArango>();
            arango.CreateCollection(name, CollectionType.Document);
        }

        private void TrySetDocument(string id, JsonObject document)
        {
            Debug.Log("Setting document: " + document.ToString());

            var i = DocumentId.Parse(id);

            document["_key"] = i.Key;
            document.Remove("_id");
            
            DB.Query(@"
                INSERT @document INTO @@collection OPTIONS { overwrite: true }
            ")
                .Bind("document", document)
                .Bind("@collection", i.Collection)
                .Run();
        }
    }
}