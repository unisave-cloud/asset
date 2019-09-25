using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Database;
using Unisave.Database.Query;

namespace Unisave.Editor.Tests.Database.Support.DatabaseProxy
{
    /// <summary>
    /// Wrapper around database proxy connection
    /// that handles JSON serialization and implements IDatabase
    ///
    /// (database proxy client cannot use framework classes)
    /// </summary>
    public class DatabaseProxyConnection : IDatabase
    {
        private global::DatabaseProxy.DatabaseProxyConnection client;

        public DatabaseProxyConnection()
        {
            client = new global::DatabaseProxy.DatabaseProxyConnection();
        }

        public void Open(string executionId, string ipAddress, int port)
        {
            client.Open(executionId, ipAddress, port);
        }

        public void Close()
        {
            client.Close();
        }

        public void SaveEntity(RawEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException();

            JsonObject jsonEntity = entity.ToJson();
            
            client.SaveEntity(jsonEntity);

            if (entity.id == null)
            {
                entity.id = jsonEntity["id"].AsString;
                entity.createdAt = DateTime.Parse(jsonEntity["createdAt"].AsString);
            }

            entity.updatedAt = DateTime.Parse(jsonEntity["updatedAt"].AsString);
            entity.ownerIds = EntityOwnerIds.FromJson(jsonEntity["ownerIds"]);
        }

        public RawEntity LoadEntity(string id)
        {
            if (id == null)
                throw new ArgumentNullException();
            
            return RawEntity.FromJson(
                client.LoadEntity(id)
            );
        }

        public IEnumerable<string> GetEntityOwners(string entityId)
        {
            if (entityId == null)
                throw new ArgumentNullException();

            return client.GetEntityOwners(entityId);
        }

        public bool IsEntityOwner(string entityId, string playerId)
        {
            if (entityId == null || playerId == null)
                throw new ArgumentNullException();

            return client.IsEntityOwner(entityId, playerId);
        }

        public bool DeleteEntity(string id)
        {
            return client.DeleteEntity(id);
        }

        public IEnumerable<RawEntity> QueryEntities(EntityQuery query)
        {
            if (query == null)
                throw new ArgumentNullException();

            return client.QueryEntities(query.ToJson())
                .Select(e => RawEntity.FromJson(e));
        }
    }
}