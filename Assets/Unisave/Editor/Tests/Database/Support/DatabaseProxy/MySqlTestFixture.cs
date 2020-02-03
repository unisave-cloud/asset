using LightJson;
using LightJson.Serialization;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using Unisave.Contracts;
using Unisave.Database;
using Unisave.Utils;

namespace Unisave.Editor.Tests.Database.Support.DatabaseProxy
{
    public class MySqlTestFixture
    {
        /// <summary>
        /// Connection to the MySQL database for making assertions
        /// </summary>
        private MySqlConnection databaseConnection;
        
        /// <summary>
        /// Connection to the database proxy for implementing IDatabase
        /// </summary>
        private DatabaseProxyConnection proxyConnection;

        /// <summary>
        /// ID of the Unisave database we are connected to
        /// </summary>
        private string databaseId;

        /// <summary>
        /// ID of the game we are working in
        /// </summary>
        private string gameId;
        
        /// <summary>
        /// Execution ID of this run
        /// </summary>
        public string ExecutionId { get; private set; }

        /// <summary>
        /// Database interface
        /// </summary>
        public IDatabase Database => proxyConnection;
        
        public void SetUp()
        {
            // setup database connection
            databaseConnection = MySqlDatabase.OpenConnection(
                Config.MySqlConnectionString
            );

            // prepare database content
            MySqlDatabase.PrepareDatabase(
                databaseConnection,
                out string _, out gameId,
                out databaseId, out string executionId
            );

            // store exec ID
            ExecutionId = executionId;
            
            // setup proxy connection
            proxyConnection = new DatabaseProxyConnection();
            proxyConnection.Open(
                executionId,
                Config.DatabaseProxyIp,
                Config.DatabaseProxyPort
            );
        }

        public void TearDown()
        {
            // disconnect from MySQL
            databaseConnection?.Close();
            databaseConnection = null;

            // disconnect from proxy
            proxyConnection?.Close();
            proxyConnection = null;
        }
        
        /////////////////////////
        // Database operations //
        /////////////////////////

        /// <summary>
        /// Creates a new player and returns their ID
        /// </summary>
        public string CreatePlayer()
        {
            string playerId = Str.Random(16);
            
            using (var command = databaseConnection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO players (
                        id, database_id, email, password, maintenance_mode
                    ) VALUES (
                        @id, @database_id, @email, 'password', FALSE
                    );
                ";
                command.Parameters.AddWithValue("id", playerId);
                command.Parameters.AddWithValue("database_id", databaseId);
                command.Parameters.AddWithValue(
                    "email", playerId + "@unisave.cloud"
                );
                command.ExecuteNonQuery();
            }

            return playerId;
        }
        
        /// <summary>
        /// Creates new entity in a new database and returns the entity id
        /// </summary>
        public string CreateEntityInAnotherDatabase(string type = "SomeType")
        {
            // create database
            string newDatabaseId = Str.Random(8);
            using (var command = databaseConnection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO `databases` (
                        id, game_id, title
                    ) 
                    VALUES (
                        @id, @game_id, 'AnotherDatabase'
                    );
                ";
                command.Parameters.AddWithValue("id", newDatabaseId);
                command.Parameters.AddWithValue("game_id", gameId);
                command.ExecuteNonQuery();
            }

            // create entity
            string entityId = Str.Random(16);
            using (var command = databaseConnection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO entities (
                        id, database_id, type, data, created_at, updated_at
                    ) VALUES (
                        @id, @database_id, @type, '{}', NOW(), NOW()
                    );
                ";
                command.Parameters.AddWithValue("id", entityId);
                command.Parameters.AddWithValue("database_id", newDatabaseId);
                command.Parameters.AddWithValue("type", type);
                command.ExecuteNonQuery();
            }

            return entityId;
        }
        
        ////////////////
        // Assertions //
        ////////////////

        /// <summary>
        /// Retrieves a given entity row from the database as a json object
        /// Or returns JSON null if row not present
        /// </summary>
        public JsonObject GetEntityRow(string id)
        {
            using (var command = databaseConnection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT JSON_OBJECT(
                        'id', id,
                        'database_id', database_id,
                        'type', type,
                        'data', data,
                        'created_at', created_at,
                        'updated_at', updated_at
                    ) AS `row` FROM entities WHERE id = @id;
                ";
                
                command.Parameters.AddWithValue("id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return JsonValue.Null;

                    return JsonReader.Parse(reader.GetString("row")).AsJsonObject;
                }
            }
        }
        
        /// <summary>
        /// Checks that an entity has been created inside proper database
        /// </summary>
        /// <param name="actualId">Database id of the entity</param>
        public void AssertDatabaseIdMatches(string actualId)
        {
            Assert.AreEqual(
                databaseId, actualId,
                "Database ID differs from what's expected."
            );
        }
        
        /// <summary>
        /// Asserts that a given player owns a given entity
        /// </summary>
        public void AssertPlayerOwns(string playerId, string entityId)
        {
            Assert.IsTrue(
                OwnershipExists(playerId, entityId),
                "Expected ownership does not exist."
            );
        }

        /// <summary>
        /// Asserts that a given player does not own a given entity
        /// </summary>
        public void AssertPlayerNotOwns(string playerId, string entityId)
        {
            Assert.IsFalse(
                OwnershipExists(playerId, entityId),
                "Unexpected ownership does exist."
            );
        }
        
        private bool OwnershipExists(string playerId, string entityId)
        {
            using (var command = databaseConnection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT * FROM entities_players
                    WHERE entity_id = @entity_id AND player_id = @player_id;
                ";
                
                command.Parameters.AddWithValue("entity_id", entityId);
                command.Parameters.AddWithValue("player_id", playerId);

                using (var reader = command.ExecuteReader())
                    return reader.Read();
            }
        }
    }
}