using MySql.Data.MySqlClient;
using NUnit.Framework;
using Unisave.Database;

namespace Unisave.Editor.Tests.Database.Support
{
    /// <summary>
    /// Base class for database test fixtures
    /// </summary>
    public class DatabaseTestFixture
    {
        /// <summary>
        /// Connection to the MySQL database for making assertions
        /// </summary>
        private MySqlConnection MySqlConnection { get; set; }
        
        /// <summary>
        /// Connection to the database proxy for implementing IDatabase
        /// </summary>
        private UnisaveDatabase proxyConnection;

        /// <summary>
        /// Interface to the underlying database
        /// </summary>
        protected IDatabase Database =>
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Config.Mode == TestMode.DatabaseProxy ? proxyConnection : null;

        [SetUp]
        public void SetUp()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (Config.Mode == TestMode.DatabaseProxy)
            {
                // setup database connection
                MySqlConnection = MySqlDatabase.OpenConnection(
                    Config.MySqlConnectionString
                );

                // prepare database content
                string execId = MySqlDatabase.PrepareDatabase(MySqlConnection);
                
                // setup proxy connection
                //proxyConnection = new DatabaseProxyConnection();
                proxyConnection = new UnisaveDatabase();
                proxyConnection.Connect(
                    execId,
                    Config.DatabaseProxyIp,
                    Config.DatabaseProxyPort
                );
            }
        }

        [TearDown]
        public void TearDown()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (Config.Mode == TestMode.DatabaseProxy)
            {
                // disconnect from MySQL
                MySqlConnection?.Close();
                MySqlConnection = null;

                // disconnect from proxy
                proxyConnection?.Disconnect();
                proxyConnection = null;
            }
        }
        
        /// <summary>
        /// Asserts that the database contains an entity with a given id
        /// </summary>
        public void AssertDatabaseHasEntity(string id)
        {
            using (var command = MySqlConnection.CreateCommand())
            {
                command.CommandText =
                    "SELECT * FROM entities WHERE id = @id;";
                command.Parameters.AddWithValue("id", id);
                using (var reader = command.ExecuteReader())
                    Assert.IsTrue(
                        reader.Read(),
                        $"Database is missing entity {id}."
                    );
            }
        }
    }
}