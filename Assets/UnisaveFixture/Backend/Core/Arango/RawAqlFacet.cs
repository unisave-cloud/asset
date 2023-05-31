using System.Collections.Generic;
using LightJson;
using Unisave.Facades;
using Unisave.Facets;
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
        
        public void Run(string aql)
        {
            DB.Query(aql).Run();
        }
        
        public string FirstAsString(string aql)
        {
            return DB.Query(aql).FirstAs<string>();
        }

        public Vector3 FirstAsVector(string aql)
        {
            return DB.Query(aql).FirstAs<Vector3>();
        }
    }
}