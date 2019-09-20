using System;
using System.Collections.Generic;
using LightJson;
using LightJson.Serialization;
using Parrot;
using Unisave.Database;

namespace Unisave.Editor.Tests.Database.Support.DatabaseProxy
{
    /// <summary>
    /// Connection to the database proxy
    /// Implements the Framework database interface
    /// </summary>
    public class DatabaseProxyConnection : IDatabase
    {
        private Client client;

        public void Open(string executionId, string ipAddress, int port)
        {
            client = Client.Connect(ipAddress, port);

            // authenticate context
            client.SendTextMessage(101, new JsonObject()
                .Add("context_type", "user")
                .Add("execution_id", executionId)
                .ToString()
            );

            // authentication succeeded
            client.ReceiveMessageType(102);
        }

        public void Close()
        {
            if (client == null)
                return;

            // exit context
            client.SendMessage(104);
            
            client.Close();
        }

        public void SaveEntity(RawEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException();

            // save entity
            client.SendTextMessage(
                201,
                new JsonObject()
                    .Add("entity", entity.ToJson())
                    .ToString()
            );

            // save entity response
            JsonObject response = JsonReader.Parse(
                client.ReceiveTextMessageType(202)
            ).AsJsonObject;

            if (entity.id == null)
            {
                entity.id = response["entity_id"].AsString;
                entity.createdAt = DateTime.Parse(response["created_at"].AsString);
            }

            entity.updatedAt = DateTime.Parse(response["updated_at"].AsString);
        }

        public RawEntity LoadEntity(string id)
        {
            if (id == null)
                throw new ArgumentNullException();
            
            client.SendTextMessage(
                203,
                new JsonObject()
                    .Add("entity_id", id)
                    .ToString()
            );
            
            JsonObject response = JsonReader.Parse(
                client.ReceiveTextMessageType(204)
            ).AsJsonObject;

            if (response["entity"].IsNull)
                return null;

            return RawEntity.FromJson(response["entity"].AsJsonObject);
        }

        public IEnumerable<string> GetEntityOwners(string entityId)
        {
            if (entityId == null)
                throw new ArgumentNullException();
            
            return new EntityOwnersRequest(entityId, client);
        }

        public bool DeleteEntity(string id)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<RawEntity> QueryEntities(string entityType, EntityQuery query)
        {
            throw new System.NotImplementedException();
        }
    }
}