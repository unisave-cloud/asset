using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Database
{
    /// <summary>
    /// Stores all emulated databases
    /// </summary>
    public class EmulatedDatabaseRepository
    {
        private const string PlayerPrefsDatabaseKey = "Unisave.EmulatedDatabase.Instance:"; // + name
        private const string PlayerPrefsDatabaseListKey = "Unisave.EmulatedDatabase.List"; // json array of names

        /*
            Databases are primarily in the memory. When the singleton is created,
            databases are loaded from PlayerPrefs. When a database changes,
            the database is saved to PlayerPrefs.
         */

        private static EmulatedDatabaseRepository singletonInstance = null;

        /// <summary>
        /// Returns the singleton instance
        /// </summary>
        public static EmulatedDatabaseRepository GetInstance()
        {
            if (singletonInstance == null)
                singletonInstance = new EmulatedDatabaseRepository();

            return singletonInstance;
        }

        /// <summary>
        /// List of loaded databases
        /// </summary>
        private Dictionary<string, EmulatedDatabase> loadedDatabases = new Dictionary<string, EmulatedDatabase>();

        /// <summary>
        /// All existing database names
        /// </summary>
        private ISet<string> databaseList;

        /// <summary>
        /// List of deleted databases
        /// This is in case you would create a database just after deletion
        /// and you want to handle nicely old references
        /// </summary>
        private Dictionary<string, EmulatedDatabase> deletedDatabases = new Dictionary<string, EmulatedDatabase>();

        /// <summary>
        /// Called when the repository changes
        /// </summary>
        public event Action OnChange;

        private EmulatedDatabaseRepository()
        {
            LoadDatabaseList();
        }

        /// <summary>
        /// Loads the list of database names
        /// </summary>
        private void LoadDatabaseList()
        {
            string rawJsonDatabases = PlayerPrefs.GetString(PlayerPrefsDatabaseListKey, null);
            
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
        public IEnumerable<EmulatedDatabase> EnumerateDatabases()
        {
            return databaseList
                .OrderBy(name => name)
                .Select(name => GetDatabase(name));
        }

        /// <summary>
        /// Save a database
        /// </summary>
        public void SaveDatabase(EmulatedDatabase database)
        {
            // revive database
            // old reference tries to save deleted database
            if (deletedDatabases.ContainsKey(database.Name))
            {
                if (deletedDatabases[database.Name] != database)
                    throw new InvalidOperationException(
                        "Trying to save a database that has not been created by the repository."
                    );

                deletedDatabases.Remove(database.Name);

                databaseList.Add(database.Name);
                SaveDatabaseList();
            }

            // perform the save
            PlayerPrefs.SetString(
                PlayerPrefsDatabaseKey + database.Name,
                database.ToJson().ToString()
            );
            PlayerPrefs.Save();

            if (OnChange != null)
                OnChange();
        }

        /// <summary>
        /// Get, load or create a database
        /// </summary>
        public EmulatedDatabase GetDatabase(string name)
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

            var database = EmulatedDatabase.FromJson(json, name);
            database.OnChange += SaveDatabase;
            
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

            EmulatedDatabase database;

            // revive database
            if (deletedDatabases.ContainsKey(name))
            {
                database = deletedDatabases[name];
                deletedDatabases.Remove(name);
            }
            else // create fresh database
            {
                database = new EmulatedDatabase(name);
                database.OnChange += SaveDatabase;
            }
            
            loadedDatabases[name] = database;
            databaseList.Add(name);

            SaveDatabaseList();
            SaveDatabase(database);

            if (OnChange != null)
                OnChange();
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
            database.Clear(raiseChangeEvent: false);

            // remove from the list
            databaseList.Remove(name);
            SaveDatabaseList();

            // remove from PlayerPrefs
            PlayerPrefs.DeleteKey(PlayerPrefsDatabaseKey + name);
            PlayerPrefs.Save();

            // register as deleted for possible revival
            deletedDatabases[name] = database;

            if (OnChange != null)
                OnChange();
        }
    }
}
