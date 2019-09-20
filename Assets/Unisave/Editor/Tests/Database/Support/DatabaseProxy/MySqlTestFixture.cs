using LightJson;
using LightJson.Serialization;
using MySql.Data.MySqlClient;
using NUnit.Framework;
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
                out string _, out string _,
                out databaseId, out string executionId
            );
                
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
        
        ////////////////
        // Assertions //
        ////////////////

        /// <summary>
        /// Retrieves a given entity row from the database as a json object
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
                        Assert.Fail(
                            $"MySql DB doesn't contain entity with id '{id}'."
                        );

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