using System;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using Unisave;
using Unisave.Serialization;
using Unisave.Utils;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Database
{
    /// <summary>
    /// Emulated analogue to the Unisave.Database.UnisaveDatabase
    /// The database is automatically loaded and saved to player preferences
    /// </summary>
    public class EmulatedDatabase : IDatabase
    {
        public const string EmulatedPlayerId = "emulated-player-id";
        public const string EmulatedPlayerEmail = "emulated@unisave.cloud";
        public static UnisavePlayer EmulatedPlayer => new UnisavePlayer("emulated-player-id");

        public struct PlayerRecord
        {
            public string id;
            public string email;
        }

        public const string PlayerPrefsDatabaseKey = "Unisave.EmulatedDatabase.Instance:"; // + name
        public const string PlayerPrefsDatabaseListKey = "Unisave.EmulatedDatabase.List"; // json array of names

        /*
            The data is public, but that's just because I'm lazy. Refactor in the future.
         */

        /// <summary>
        /// List of all players
        /// </summary>
        public List<PlayerRecord> players = new List<PlayerRecord>();

        /// <summary>
        /// List of all entities
        /// </summary>
        public Dictionary<string, RawEntity> entities = new Dictionary<string, RawEntity>();

        /// <summary>
        /// Pairs of [ entity | player ]
        /// </summary>
        public List<Tuple<string, string>> entityOwnerships = new List<Tuple<string, string>>();

        /// <summary>
        /// Currently used database
        /// </summary>
        public string DatabaseName { get; private set; }

        /// <summary>
        /// When true, the database shouldn't be accessed
        /// (to detect client-side db access)
        /// </summary>
        private Func<bool> PreventAccess;

        public EmulatedDatabase(Func<bool> PreventAccess)
        {
            this.PreventAccess = PreventAccess;

            Clear();
        }

        /// <summary>
        /// What database to use
        /// </summary>
        public void Use(string databaseName)
        {
            if (String.IsNullOrEmpty(databaseName))
                databaseName = "null";

            if (databaseName == DatabaseName)
                return;

            // stop using current database
            if (DatabaseName != null)
            {
                SaveDatabase();
                DatabaseName = null;
            }

            Clear();

            // start using specified database
            DatabaseName = databaseName;
            LoadDatabase();
        }

        /// <summary>
        /// Empty the database
        /// </summary>
        public void Clear()
        {
            players.Clear();
            entities.Clear();
            entityOwnerships.Clear();

            // create the always present emulated player
            players.Add(new PlayerRecord {
                id = EmulatedPlayerId,
                email = EmulatedPlayerEmail
            });
        }

        /// <summary>
        /// Load database content from player prefs
        /// </summary>
        private void LoadDatabase()
        {
            string rawJson = PlayerPrefs.GetString(PlayerPrefsDatabaseKey + DatabaseName, null);

            // database does not exist yet
            if (String.IsNullOrEmpty(rawJson))
                return;

            JsonObject json = JsonReader.Parse(rawJson);

            players.AddRange(
                json["players"]
                    .AsJsonArray
                    .Select(
                        x => new PlayerRecord {
                            id = x.AsJsonObject["id"].AsString,
                            email = x.AsJsonObject["email"].AsString
                        }
                    )
            );

            var enumerable = json["entities"].AsJsonArray.Select(x => RawEntity.FromJson(x));
            foreach (RawEntity e in enumerable)
                entities.Add(e.id, e);

            entityOwnerships.AddRange(
                json["entityOwnerships"]
                    .AsJsonArray
                    .Select(
                        x => new Tuple<string, string>(
                            x.AsJsonObject["entityId"],
                            x.AsJsonObject["playerId"]
                        )
                    )
            );
        }

        /// <summary>
        /// Save database to player prefs
        /// </summary>
        private void SaveDatabase()
        {
            JsonObject json = new JsonObject();

            json["players"] = new JsonArray(
                players
                    .Select(x => (JsonValue)(
                        new JsonObject()
                            .Add("id", x.id)
                            .Add("email", x.email)
                    ))
                    .ToArray()
            );

            json["entities"] = new JsonArray(
                entities.Select(p => (JsonValue)p.Value.ToJson()).ToArray()
            );

            json["entityOwnerships"] = new JsonArray(
                entityOwnerships
                    .Select(
                        x => (JsonValue)new JsonObject()
                            .Add("entityId", x.Item1)
                            .Add("playerId", x.Item2)
                    )
                    .ToArray()
            );

            PlayerPrefs.SetString(PlayerPrefsDatabaseKey + DatabaseName, json.ToString());

            // DB list

            string rawJsonDatabases = PlayerPrefs.GetString(PlayerPrefsDatabaseListKey, null);
            if (String.IsNullOrEmpty(rawJsonDatabases))
                rawJsonDatabases = "[]";
            JsonArray jsonDatabases = JsonReader.Parse(rawJsonDatabases);

            var databases = new HashSet<string>(
                jsonDatabases.Select(x => (string)x)
            );

            databases.Add(DatabaseName);

            jsonDatabases = new JsonArray(
                databases.Select(x => (JsonValue)x).ToArray()
            );

            PlayerPrefs.SetString(PlayerPrefsDatabaseListKey, jsonDatabases.ToString());

            // save

            PlayerPrefs.Save();
        }

        /// <summary>
        /// Checks proper emulation state.
        /// Throws exception on failure
        /// </summary>
        private void GuardClientSide()
        {
            if (PreventAccess())
                FakeDatabase.NotifyDeveloper();
        }

        /// <inheritdoc/>
        public void SaveEntity(RawEntity entity)
        {
            GuardClientSide();

            if (entity.id == null)
                InsertEntity(entity);
            else
                UpdateEntity(entity);

            SaveDatabase();
        }

        private void InsertEntity(RawEntity entity)
        {
            entity.id = Str.Random(16);
            entity.updatedAt = entity.createdAt = DateTime.UtcNow;

            entities.Add(entity.id, RawEntity.FromJson(entity.ToJson()));

            AddOwners(entity.id, entity.ownerIds);
        }

        private void UpdateEntity(RawEntity entity)
        {
            entity.updatedAt = DateTime.UtcNow;

            entities[entity.id] = RawEntity.FromJson(entity.ToJson());

            RemoveAllOwners(entity.id);
            AddOwners(entity.id, entity.ownerIds);
        }

        /// <inheritdoc/>
        public RawEntity LoadEntity(string id)
        {
            GuardClientSide();

            if (!entities.ContainsKey(id))
                return null;

            var entity = RawEntity.FromJson(entities[id].ToJson());

            entity.ownerIds = new HashSet<string>(GetEntityOwners(id));

            return entity;
        }

        /// <inheritdoc/>
        public bool DeleteEntity(string id)
        {
            GuardClientSide();

            RemoveAllOwners(id);

            if (!entities.ContainsKey(id))
                return false;

            entities.Remove(id);
            SaveDatabase();

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<RawEntity> QueryEntities(string entityType, EntityQuery query)
        {
            /*
                This implementation is really not the best possible, but I didn't want to waste
                time by overly optimizing a database, that is going to have hundreds of items at most.
                (remember, this is the emulated one, not the real one)
             */

            GuardClientSide();

            // build a set of entities that are owned by all the required players
            HashSet<string> entityIds = null; // null means the entire universe

            foreach (UnisavePlayer player in query.requiredOwners)
            {
                var entityIdsOwnedByThisPlayer = entityOwnerships
                    .Where(t => t.Item2 == player.Id)
                    .Select(t => t.Item1);

                var playerEntityIds = entities
                    .Where(p => p.Value.type == entityType)
                    .Where(p => entityIdsOwnedByThisPlayer.Contains(p.Value.id))
                    .Select(p => p.Value.id);

                if (entityIds == null)
                    entityIds = new HashSet<string>(playerEntityIds);
                else
                    entityIds.IntersectWith(playerEntityIds);
            }

            // game entity is queried
            if (entityIds == null)
            {
                // super slow, but... prototyping! :D
                var ownedIds = new HashSet<string>(entityOwnerships.Select(x => x.Item1));
                entityIds = new HashSet<string>(entities.Keys.Where(x => !ownedIds.Contains(x)));
            }

            // load entities
            IEnumerable<RawEntity> loadedEntities = entityIds.Select(id => LoadEntity(id));

            // if exact, remove those owned by too many players
            if (query.requireOwnersExactly)
                loadedEntities = loadedEntities.Where(e => e.ownerIds.Count == query.requiredOwners.Count);

            // take only one
            if (query.takeFirstFound)
                return loadedEntities.Take(1);

            return loadedEntities;
        }

        /// <summary>
        /// Returns a set of owners of a given entity
        /// </summary>
        private IEnumerable<string> GetEntityOwners(string entityId)
        {
            return entityOwnerships.Where(t => t.Item1 == entityId).Select(t => t.Item2);
        }

        /// <summary>
        /// Removes all owners of an entity
        /// </summary>
        private void RemoveAllOwners(string entityId)
        {
            entityOwnerships.RemoveAll(t => t.Item1 == entityId);
        }

        /// <summary>
        /// Adds given owners to the entity.
        /// Assumes all owners are new
        /// </summary>
        private void AddOwners(string entityId, ISet<string> ownerIds)
        {
            foreach (string ownerId in ownerIds)
                entityOwnerships.Add(new Tuple<string, string>(entityId, ownerId));
        }
    }
}
