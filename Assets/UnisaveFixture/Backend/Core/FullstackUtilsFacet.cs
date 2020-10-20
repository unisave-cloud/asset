using System.Linq;
using Unisave.Arango;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Facets;
using Unisave.Sessions;

namespace UnisaveFixture.Backend.Core
{
    public class FullstackUtilsFacet : Facet
    {
        /// <summary>
        /// Deletes all the non-system collections
        /// </summary>
        public void ClearDatabase()
        {
            var arango = (ArangoConnection) Facade.App.Resolve<IArango>();

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
            Facade.App.Instance<ISession>(
                new SessionOverStorage(
                    storage: null,
                    sessionLifetime: 0
                )
            );
        }
    }
}