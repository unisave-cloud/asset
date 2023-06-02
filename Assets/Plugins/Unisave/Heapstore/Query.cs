using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightJson;
using Unisave.Facets;
using Unisave.Heapstore.Backend;
using UnityEngine;

namespace Unisave.Heapstore
{
    /// <summary>
    /// Lets you build a heapstore query and execute it
    /// </summary>
    public class Query
    {
        protected readonly QueryRequest request;
        protected readonly MonoBehaviour caller;

        public Query(string collectionName, MonoBehaviour caller = null)
            : this(new QueryRequest { collection = collectionName}, caller) { }

        public Query(QueryRequest request, MonoBehaviour caller = null)
        {
            this.request = request;
            this.caller = caller;
        }
        
        //////////////
        // Building //
        //////////////

        public Query Filter(string field, string op, object value)
        {
            return this;
        }
        
        
        ///////////////
        // Execution //
        ///////////////
        
        public UnisaveOperation<List<Document>> Get()
        {
            return new UnisaveOperation<List<Document>>(caller, GetAsync());
        }

        private async Task<List<Document>> GetAsync()
        {
            List<JsonObject> fetchedJson = await caller.CallFacet(
                (HeapstoreFacet f) => f.ExecuteQuery(request)
            );

            return fetchedJson.Select(o => new Document(o)).ToList();
        }
    }
}