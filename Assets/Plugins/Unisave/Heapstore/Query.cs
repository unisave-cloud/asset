using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightJson;
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

        public Query Sort(string field, string direction)
        {
            return Sort(
                (field, direction)
            );
        }
        
        public Query Sort(params ValueTuple<string, string>[] fieldsAndDirections)
        {
            return this;
        }

        public Query Limit(int take)
        {
            return Limit(skip: 0, take: take);
        }

        public Query Limit(int skip, int take)
        {
            return this;
        }
        
        
        ///////////////
        // Execution //
        ///////////////
        
        /// <summary>
        /// Execute the query and get all the matching documents
        /// </summary>
        public UnisaveOperation<List<Document>> Get()
        {
            return new UnisaveOperation<List<Document>>(caller, GetAsync());
        }

        /// <summary>
        /// Execute the query and get all the documents
        /// converted to the given type
        /// </summary>
        /// <typeparam name="T">The type to convert the documents to</typeparam>
        public UnisaveOperation<List<T>> GetAs<T>()
        {
            return new UnisaveOperation<List<T>>(caller, async () => {
                List<Document> documents = await GetAsync();
                return documents.Select(d => d.As<T>()).ToList();
            });
        }

        /// <summary>
        /// Execute the query and return the first document to match,
        /// or null if no documents match
        /// </summary>
        public UnisaveOperation<Document> First()
        {
            // TODO: add a Limit(1) clause (or rather, set the "take", keep "skip")
            
            return new UnisaveOperation<Document>(caller, async () => {
                List<Document> documents = await GetAsync();
                if (documents.Count == 0)
                    return null;
                return documents[0];
            });
        }

        /// <summary>
        /// Execute the query and return the first document to match,
        /// or null if no documents match. Then convert the document
        /// to the given type.
        /// </summary>
        /// <typeparam name="T">The type to convert the document to</typeparam>
        public UnisaveOperation<T> FirstAs<T>()
        {
            return new UnisaveOperation<T>(caller, async () => {
                Document document = await First();
                if (document == null)
                    return default(T);
                return document.As<T>();
            });
        }

        private async Task<List<Document>> GetAsync()
        {
            var transport = new TransportLayer(caller);
            
            List<JsonObject> fetchedJson = await transport.Call(f =>
                f.ExecuteQuery(request)
            );

            return fetchedJson.Select(o => new Document(o)).ToList();
        }
    }
}