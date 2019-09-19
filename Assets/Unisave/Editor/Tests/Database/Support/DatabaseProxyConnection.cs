using System;
using System.Collections.Generic;
using Unisave.Database;
using Parrot;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Editor.Tests.Database.Support
{
    /// <summary>
    /// Connection to the database proxy
    /// Implements the Framework database interface
    /// </summary>
    public class DatabaseProxyConnection : IDatabase
    {
        private Client client;

        public DatabaseProxyConnection() { }

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
            client.SendTextMessage(201, entity.ToJson().ToString());

            // save entity response
            JsonObject response = JsonReader.Parse(client.ReceiveTextMessageType(202)).AsJsonObject;

            if (entity.id == null)
            {
                entity.id = response["entity_id"].AsString;
                entity.createdAt = DateTime.Parse(response["created_at"].AsString);
            }

            entity.updatedAt = DateTime.Parse(response["updated_at"].AsString);
        }

        public RawEntity LoadEntity(string id)
        {
            throw new System.NotImplementedException();
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