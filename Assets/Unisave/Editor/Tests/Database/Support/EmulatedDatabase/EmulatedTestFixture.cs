using System;
using LightJson;

namespace Unisave.Editor.Tests.Database.Support.EmulatedDatabase
{
    public class EmulatedTestFixture
    {
        public void SetUp()
        {
            throw new NotImplementedException();
        }

        public void TearDown()
        {
            throw new NotImplementedException();
        }
        
        /////////////////////////
        // Database operations //
        /////////////////////////

        /// <summary>
        /// Creates a new player and returns their ID
        /// </summary>
        public string CreatePlayer()
        {
            throw new NotImplementedException();
        }
        
        ////////////////
        // Assertions //
        ////////////////

        /// <summary>
        /// Retrieves a given entity row from the database as a json object
        /// </summary>
        public JsonObject GetEntityRow(string id)
        {
            // provide some fake data for the "database_id" column
            
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asserts that a given player does not own a given entity
        /// </summary>
        public void AssertPlayerNotOwns(string playerId, string entityId)
        {
            throw new NotImplementedException();
        }
    }
}