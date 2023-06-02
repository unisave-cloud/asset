using System;
using System.Collections.Generic;
using LightJson;
using Unisave.Arango;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Facets;

namespace Unisave.Heapstore.Backend
{
    public class HeapstoreFacet : Facet
    {
        #region "Query API"
        
        
        /////////////////////
        // Query execution //
        /////////////////////

        public List<JsonObject> ExecuteQuery(QueryRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            
            try
            {
                return request.BuildAqlQuery().GetAs<JsonObject>();
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                return new List<JsonObject>();
            }
        }
        
        #endregion
        
        #region "Document API"
        
        
        ///////////////////
        // Get operation //
        ///////////////////
        
        public JsonObject GetDocument(DocumentId id)
        {
            try
            {
                return DB.Query(@"
                    RETURN DOCUMENT(@id)
                ")
                    .Bind("id", id.Id)
                    .FirstAs<JsonObject>();
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                return null;
            }
        }
        
        
        ///////////////////
        // Set operation //
        ///////////////////
        
        public JsonObject SetDocument(DocumentId id, JsonObject document)
        {
            try
            {
                return TrySetDocument(id, document);
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                CreateCollection(id.Collection);
                return TrySetDocument(id, document);
            }
        }

        private JsonObject TrySetDocument(DocumentId id, JsonObject document)
        {
            document["_key"] = id.Key;
            document.Remove("_id");
            
            return DB.Query(@"
                INSERT @document INTO @@collection OPTIONS { overwrite: true }
                RETURN NEW
            ")
                .Bind("document", document)
                .Bind("@collection", id.Collection)
                .FirstAs<JsonObject>();
        }
        
        
        ///////////////
        // Utilities //
        ///////////////

        private void CreateCollection(string name)
        {
            var arango = (ArangoConnection) Facade.App.Resolve<IArango>();
            arango.CreateCollection(name, CollectionType.Document);
        }
        
        #endregion
    }
}