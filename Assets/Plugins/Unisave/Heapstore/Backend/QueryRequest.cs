using System.Collections.Generic;
using System.Text;
using LightJson;
using Unisave.Contracts;
using Unisave.Facades;

namespace Unisave.Heapstore.Backend
{
    /// <summary>
    /// Represents a query request, executable by heapstore
    /// </summary>
    public class QueryRequest
    {
        /// <summary>
        /// Name of the collection to query
        /// </summary>
        public string collection;
        
        // TODO: filter clauses
        
        // TODO: sort clause
        
        // TODO: limit clause

        // TODO: validate values
        
        public IAqlQuery BuildAqlQuery()
        {
            // accumulate terms
            var bindings = new Dictionary<string, JsonValue>();
            var sb = new StringBuilder();

            // translate collection selector
            sb.AppendLine("FOR doc IN @@collection");
            bindings["@collection"] = collection;
            
            // TODO: translate intermediate clauses
            
            // translate projection (no projection)
            sb.AppendLine("RETURN doc");

            // build the query
            var aqlQuery = DB.Query(sb.ToString());
            foreach (var x in bindings)
                aqlQuery.Bind(x.Key, x.Value);
            return aqlQuery;
        }
    }
}