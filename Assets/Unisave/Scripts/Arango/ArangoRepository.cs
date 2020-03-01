using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using LightJson.Serialization;
using Unisave.Arango.Emulation;
using UnityEngine;

namespace Unisave.Arango
{
    /// <summary>
    /// Repository of all available arango (in memory) databases
    /// </summary>
    public class ArangoRepository
    {
        /// <summary>
        /// PlayerPrefs key prefix for a single arango database
        /// </summary>
        private const string PlayerPrefsDatabaseKey
            = "Unisave.EmulatedDatabase.Instance:"; // + name
        
        /// <summary>
        /// PlayerPrefs key for the list of all databases
        /// </summary>
        private const string PlayerPrefsDatabaseListKey
            = "Unisave.EmulatedDatabase.List"; // json array of names

        /*
            Databases are primarily in the memory. When the singleton is created,
            databases are loaded from PlayerPrefs. When a database changes,
            the database is saved to PlayerPrefs.
         */

        private static ArangoRepository singletonInstance;

        /// <summary>
        /// Returns the singleton instance
        /// </summary>
        public static ArangoRepository GetInstance()
        {
            if (singletonInstance == null)
                singletonInstance = new ArangoRepository();

            return singletonInstance;
        }

        /// <summary>
        /// List of loaded databases
        /// </summary>
        private Dictionary<string, ArangoInMemory> loadedDatabases
            = new Dictionary<string, ArangoInMemory>();

        /// <summary>
        /// All existing database names
        /// </summary>
        private ISet<string> databaseList;

        /// <summary>
        /// List of deleted databases
        /// This is in case you would create a database just after deletion
        /// and you want to handle nicely old references
        /// </summary>
        private Dictionary<string, ArangoInMemory> deletedDatabases
            = new Dictionary<string, ArangoInMemory>();

        /// <summary>
        /// Called when the repository changes
        /// </summary>
        public event Action OnChange;

        private ArangoRepository()
        {
            LoadDatabaseList();
        }

        /// <summary>
        /// Loads the list of database names
        /// </summary>
        private void LoadDatabaseList()
        {
            string rawJsonDatabases = PlayerPrefs.GetString(
                PlayerPrefsDatabaseListKey,
                null
            );
            
            if (String.IsNullOrEmpty(rawJsonDatabases))
                rawJsonDatabases = "[]";

            JsonArray jsonDatabases = JsonReader.Parse(rawJsonDatabases);

            databaseList = new HashSet<string>(
                jsonDatabases.Select(x => (string)x)
            );
        }

        /// <summary>
        /// Saves the list of database names
        /// </summary>
        private void SaveDatabaseList()
        {
            PlayerPrefs.SetString(
                PlayerPrefsDatabaseListKey,
                new JsonArray(
                    databaseList.Select(x => (JsonValue)x).ToArray()
                ).ToString()
            );
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Enumerate all existing databases ordered by their name
        /// </summary>
        public IEnumerable<KeyValuePair<string, ArangoInMemory>>
            EnumerateDatabases()
        {
            return databaseList
                .OrderBy(name => name)
                .Select(name => new KeyValuePair<string, ArangoInMemory>(
                    name,
                    GetDatabase(name)
                ));
        }

        /// <summary>
        /// Save a database
        /// </summary>
        public void SaveDatabase(string name, ArangoInMemory database)
        {
            // revive database
            // old reference tries to save deleted database
            if (deletedDatabases.ContainsKey(name))
            {
                if (deletedDatabases[name] != database)
                    throw new InvalidOperationException(
                        "Trying to save a database that has not been created by the repository."
                    );

                deletedDatabases.Remove(name);

                databaseList.Add(name);
                SaveDatabaseList();
            }

            // perform the save
            PlayerPrefs.SetString(
                PlayerPrefsDatabaseKey + name,
                database.ToJson().ToString()
            );
            PlayerPrefs.Save();

            OnChange?.Invoke();
        }

        /// <summary>
        /// Get, load or create a database
        /// </summary>
        public ArangoInMemory GetDatabase(string name)
        {
            // if already loaded
            if (loadedDatabases.ContainsKey(name))
                return loadedDatabases[name];

            // if exists but not loaded
            if (databaseList.Contains(name))
            {
                LoadDatabase(name);
                return loadedDatabases[name];
            }

            // if not even exists
            CreateDatabase(name);
            return loadedDatabases[name];
        }

        /// <summary>
        /// Load database from PlayerPrefs to the dictionary
        /// </summary>
        private void LoadDatabase(string name)
        {
            if (loadedDatabases.ContainsKey(name))
                throw new InvalidOperationException("Database already loaded.");

            JsonObject json = JsonReader.Parse(
                PlayerPrefs.GetString(PlayerPrefsDatabaseKey + name, "{}")
            );

            var database = ArangoInMemory.FromJson(json);
            //database.OnChange += SaveDatabase; // TODO: onChange
            
            loadedDatabases[name] = database;
        }

        /// <summary>
        /// Create database of a given name
        /// </summary>
        private void CreateDatabase(string name)
        {
            if (loadedDatabases.ContainsKey(name))
                throw new InvalidOperationException("Database already loaded.");

            if (databaseList.Contains(name))
                throw new InvalidOperationException("Database already exists.");

            ArangoInMemory database;

            // revive database
            if (deletedDatabases.ContainsKey(name))
            {
                database = deletedDatabases[name];
                deletedDatabases.Remove(name);
            }
            else // create fresh database
            {
                database = new ArangoInMemory();
                //database.OnChange += SaveDatabase; // TODO: on change
            }
            
            loadedDatabases[name] = database;
            databaseList.Add(name);

            SaveDatabaseList();
            SaveDatabase(name, database);

            OnChange?.Invoke();
        }

        /// <summary>
        /// Delete a database
        /// </summary>
        public void DeleteDatabase(string name)
        {
            // NOTE: already deleted database will get revived first and then deleted again

            // NOTE: change event is not forgotten since we want to revive the database in such case

            var database = GetDatabase(name);
            
            // clear to make sure any left over references won't see the deleted data
            //database.Clear(raiseChangeEvent: false); // TODO
            database.Clear();

            // remove from the list
            databaseList.Remove(name);
            SaveDatabaseList();

            // remove from PlayerPrefs
            PlayerPrefs.DeleteKey(PlayerPrefsDatabaseKey + name);
            PlayerPrefs.Save();

            // register as deleted for possible revival
            deletedDatabases[name] = database;

            OnChange?.Invoke();
        }
    }
}
