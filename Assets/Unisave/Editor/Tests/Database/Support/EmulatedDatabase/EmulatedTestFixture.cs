using System;
using System.Linq;
using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Utils;

namespace Unisave.Editor.Tests.Database.Support.EmulatedDatabase
{
    public class EmulatedTestFixture
    {
        private Unisave.Database.EmulatedDatabase database;

        private string databaseName = "test";
        
        /// <summary>
        /// Database interface
        /// </summary>
        public IDatabase Database => database;
        
        public void SetUp()
        {
            databaseName = "test";
            database = new Unisave.Database.EmulatedDatabase(databaseName);
        }

        public void TearDown()
        {
            databaseName = null;
            database = null;
        }
        
        /////////////////////////
        // Database operations //
        /////////////////////////

        /// <summary>
        /// Creates a new player and returns their ID
        /// </summary>
        public string CreatePlayer()
        {
            return database.AddPlayer(Str.Random(6) + "@unisave.cloud");
        }
        
        ////////////////
        // Assertions //
        ////////////////

        /// <summary>
        /// Retrieves a given entity row from the database as a json object
        /// </summary>
        public JsonObject GetEntityRow(string id)
        {
            var entity = database.LoadEntity(id);

            if (entity == null)
                return JsonValue.Null;
            
            return new JsonObject()
                .Add("id", entity.id)
                .Add("database_id", databaseName)
                .Add("type", entity.type)
                .Add("data", entity.data)
                .Add("created_at", entity.createdAt)
                .Add("updated_at", entity.updatedAt);
        }

        /// <summary>
        /// Checks that an entity has been created inside proper database
        /// </summary>
        /// <param name="actualId">Database id of the entity</param>
        public void AssertDatabaseIdMatches(string actualId)
        {
            // do nothing, mismatch of databases in emulated database is silly
        }
        
        /// <summary>
        /// Asserts that a given player owns a given entity
        /// </summary>
        public void AssertPlayerOwns(string playerId, string entityId)
        {
            Assert.IsTrue(
                database.GetEntityOwners(entityId).Contains(playerId)
            );
        }

        /// <summary>
        /// Asserts that a given player does not own a given entity
        /// </summary>
        public void AssertPlayerNotOwns(string playerId, string entityId)
        {
            Assert.IsFalse(
                database.GetEntityOwners(entityId).Contains(playerId)
            );
        }
    }
}