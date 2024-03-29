using System.Collections.Generic;
using LightJson;
using Unisave.Arango;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Facets;
using Unisave.Foundation;
using UnityEngine;

namespace UnisaveFixture.Backend.Core.Arango
{
    public class RawAqlFacet : Facet
    {
        public List<JsonValue> Get(string aql, JsonObject bindParams)
        {
            if (bindParams == null)
                bindParams = new JsonObject();
            
            var query = DB.Query(aql);
            
            foreach (var pair in bindParams)
                query.Bind(pair.Key, pair.Value);

            return query.Get();
        }
        
        public void CreateCollection(string name)
        {
            var arango = (ArangoConnection) RequestContext.Current.Services.Resolve<IArango>();
            arango.CreateCollection(name, CollectionType.Document);
        }
        
        public void Run(string aql)
        {
            DB.Query(aql).Run();
        }
        
        public JsonValue First(string aql)
        {
            return DB.Query(aql).First();
        }

        public Vector3 FirstAsVector(string aql)
        {
            return DB.Query(aql).FirstAs<Vector3>();
        }
    }
}