using System;
using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Editor.Tests.Database.Support.DatabaseProxy;
using Unisave.Editor.Tests.Database.Support.EmulatedDatabase;

namespace Unisave.Editor.Tests.Database.Support
{
    /// <summary>
    /// Base class for database test fixtures
    ///
    /// Routes all actions to MySqlTestFixture or EmulatedTestFixture
    /// </summary>
    public class DatabaseTestFixture
    {
        // underlying fixtures
        // protected because they can be accessed by mode-specific tests
        protected MySqlTestFixture mySqlTestFixture;
        protected EmulatedTestFixture emulatedTestFixture;

        /// <summary>
        /// What mode are we running the tests in
        /// </summary>
        protected TestMode Mode => Config.Mode;
        
        /// <summary>
        /// Interface to the underlying database
        /// </summary>
        protected IDatabase Database =>
            Mode == TestMode.DatabaseProxy
                ? mySqlTestFixture.Database
                : null;

        [SetUp]
        public void SetUp()
        {
            if (Mode == TestMode.DatabaseProxy)
            {
                mySqlTestFixture = new MySqlTestFixture();
                mySqlTestFixture.SetUp();
            }
            
            if (Mode == TestMode.EmulatedDatabase)
            {
                emulatedTestFixture = new EmulatedTestFixture();
                emulatedTestFixture.SetUp();
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (Mode == TestMode.DatabaseProxy)
            {
                mySqlTestFixture.TearDown();
                mySqlTestFixture = null;
            }
            
            if (Mode == TestMode.EmulatedDatabase)
            {
                emulatedTestFixture.TearDown();
                emulatedTestFixture = null;
            }
        }
        
        /////////////////////////
        // Database operations //
        /////////////////////////

        /// <summary>
        /// Creates a new player and returns their ID
        /// </summary>
        protected string CreatePlayer()
        {
            if (Mode == TestMode.DatabaseProxy)
                return mySqlTestFixture.CreatePlayer();
            
            if (Mode == TestMode.EmulatedDatabase)
                return emulatedTestFixture.CreatePlayer();

            throw new NotImplementedException();
        }
        
        ///////////////////////
        // Shared assertions //
        ///////////////////////
        
        /// <summary>
        /// Asserts that the database contains an entity with a given id
        /// </summary>
        public void AssertDatabaseHasEntity(string id)
        {
            GetEntityRow(id);
        }

        /// <summary>
        /// Retrieves a given entity row from the database as a json object
        /// </summary>
        public JsonObject GetEntityRow(string id)
        {
            if (Mode == TestMode.DatabaseProxy)
                return mySqlTestFixture.GetEntityRow(id);
            
            if (Mode == TestMode.EmulatedDatabase)
                return emulatedTestFixture.GetEntityRow(id);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks that an entity has been created inside proper database
        /// </summary>
        /// <param name="actualId">Database id of the entity</param>
        public void AssertDatabaseIdMatches(string actualId)
        {
            if (Mode == TestMode.DatabaseProxy)
                mySqlTestFixture.AssertDatabaseIdMatches(actualId);
            
            if (Mode == TestMode.EmulatedDatabase)
                emulatedTestFixture.AssertDatabaseIdMatches(actualId);
        }

        /// <summary>
        /// Asserts that a given player owns a given entity
        /// </summary>
        public void AssertPlayerOwns(string playerId, string entityId)
        {
            if (Mode == TestMode.DatabaseProxy)
                mySqlTestFixture.AssertPlayerOwns(playerId, entityId);
            
            if (Mode == TestMode.EmulatedDatabase)
                emulatedTestFixture.AssertPlayerOwns(playerId, entityId);
        }

        /// <summary>
        /// Asserts that a given player does not own a given entity
        /// </summary>
        public void AssertPlayerNotOwns(string playerId, string entityId)
        {
            if (Mode == TestMode.DatabaseProxy)
                mySqlTestFixture.AssertPlayerNotOwns(playerId, entityId);
            
            if (Mode == TestMode.EmulatedDatabase)
                emulatedTestFixture.AssertPlayerNotOwns(playerId, entityId);
        }
    }
}