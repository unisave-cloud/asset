namespace Unisave.Editor.Tests.Database.Support
{
    /// <summary>
    /// Configuration for the database tests
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Should the tests run against real database proxy
        /// Or against the emulated database in memory
        /// </summary>
        public const TestMode Mode = TestMode.EmulatedDatabase;
        
        ///////////////////////////////////////
        // Database proxy mode configuration //
        ///////////////////////////////////////

        public const string MySqlConnectionString
            = "server=localhost;port=3306;user id=root; password=; "
                + "database=unisave_test; SslMode=none";

        public const string DatabaseProxyIp = "127.0.0.1";

        public const int DatabaseProxyPort = 7777;
    }

    public enum TestMode
    {
        DatabaseProxy,
        EmulatedDatabase
    }
}