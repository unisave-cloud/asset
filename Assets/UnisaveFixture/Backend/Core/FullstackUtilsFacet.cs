using System.Linq;
using Unisave.Arango;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Facets;
using Unisave.Foundation;
using Unisave.Sessions;
using Unisave.Sessions.Storage;

namespace UnisaveFixture.Backend.Core
{
    public class FullstackUtilsFacet : Facet
    {
        private IContainer services;

        public FullstackUtilsFacet(IContainer services)
        {
            this.services = services;
        }

        /// <summary>
        /// Deletes all the non-system collections
        /// </summary>
        public void ClearDatabase()
        {
            var arango = (ArangoConnection) services.Resolve<IArango>();

            var collections = arango.Get("/_api/collection")["result"].AsJsonArray;

            var nonSystemCollectionNames = collections
                .Select(c => c["name"].AsString)
                .Where(c => c[0] != '_')
                .ToList();
            
            foreach (string name in nonSystemCollectionNames)
                arango.DeleteCollection(name);
            
            PreventSessionFromCreatingCollection();
        }

        private void PreventSessionFromCreatingCollection()
        {
            // automatically a per-request singleton,
            // since this is the request-scoped container
            
            services.RegisterSingleton<ISessionStorage, InMemorySessionStorage>();

            services.RegisterSingleton<ISession>(
                container => new SessionFrontend(
                    container.Resolve<ISessionStorage>(),
                    0
                )
            );
        }
    }
}