using System.Collections;
using System.Collections.Generic;
using Unisave.Contracts;
using UnityEngine;
using Unisave.Database;
using Unisave.Database.Query;
using Unisave.Exceptions;

namespace Unisave.Database
{
    /// <summary>
    /// This class is placed at the framework endpoint to notify the developer
    /// whenever they try to access the database locally
    /// </summary>
    public class FakeDatabase : IDatabase
    {
        public static void NotifyDeveloper()
        {
            throw new UnisaveException(
                "You cannot query, load or save entities from client-side " +
                "code. This can only be done from inside facets.\n" +
                "You can however return entities from facets, " +
                "so you can work with them on the client side."
            );
        }

        /// <inheritdoc/>
        public void SaveEntity(RawEntity entity)
        {
            NotifyDeveloper();
        }

        /// <inheritdoc/>
        public RawEntity LoadEntity(string id, string lockType = null)
        {
            NotifyDeveloper();
            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetEntityOwners(string entityId)
        {
            NotifyDeveloper();
            return new string[0];
        }
        
        /// <inheritdoc/>
        public bool IsEntityOwner(string entityId, string playerId)
        {
            NotifyDeveloper();
            return false;
        }

        /// <inheritdoc/>
        public bool DeleteEntity(string id)
        {
            NotifyDeveloper();
            return false;
        }

        /// <inheritdoc/>
        public IEnumerable<RawEntity> QueryEntities(EntityQuery query)
        {
            NotifyDeveloper();
            yield break;
        }

        /// <inheritdoc/>
        public void StartTransaction()
        {
            NotifyDeveloper();
        }

        /// <inheritdoc/>
        public void RollbackTransaction()
        {
            NotifyDeveloper();
        }

        /// <inheritdoc/>
        public void CommitTransaction()
        {
            NotifyDeveloper();
        }

        /// <inheritdoc/>
        public int TransactionLevel()
        {
            NotifyDeveloper();
            return 0;
        }
    }
}
