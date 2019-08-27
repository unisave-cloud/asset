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
using Unisave.Exceptions;

namespace Unisave.Database
{
    /// <summary>
    /// Emulated analogue to the Unisave.Database.UnisaveDatabase
    /// The database is automatically loaded and saved to player preferences
    /// </summary>
    public class EmulatedDatabase : IDatabase
    {
        public class PlayerRecord : IEquatable<PlayerRecord>
        {
            public string id;
            public string email;

            public bool Equals(PlayerRecord that)
            {
                return this.id == that.id;
            }
        }

        /// <summary>
        /// List of all players
        /// </summary>
        private ISet<PlayerRecord> players = new HashSet<PlayerRecord>();

        /// <summary>
        /// List of all entities
        /// </summary>
        private Dictionary<string, RawEntity> entities = new Dictionary<string, RawEntity>();

        /// <summary>
        /// Pairs of [ entity | player ]
        /// </summary>
        private List<Tuple<string, string>> entityOwnerships = new List<Tuple<string, string>>();

        /// <summary>
        /// Name of the database
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// When true, the database shouldn't be accessed
        /// (to detect client-side db access)
        /// </summary>
        public bool PreventAccess { get; set; } = false;

        /// <summary>
        /// Called after someone accesses and mutates the database via the IDatabase interface
        /// (meaning from inside the emulated server code)
        /// </summary>
        public event OnChangeEventHandler OnChange;

        /// <summary>
        /// Event handler for the AfterInterfaceAccess event
        /// </summary>
        /// <param name="subject">Database that was accessed</param>
        public delegate void OnChangeEventHandler(EmulatedDatabase subject);

        public EmulatedDatabase(string name)
        {
            this.Name = name;

            Clear();
        }

        /// <summary>
        /// Empty the database
        /// </summary>
        public void Clear(bool raiseChangeEvent = false)
        {
            players.Clear();
            entities.Clear();
            entityOwnerships.Clear();

            if (raiseChangeEvent)
                OnChange(this);
        }

        /// <summary>
        /// Serialize database to json
        /// </summary>
        public JsonObject ToJson()
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

            return json;
        }

        /// <summary>
        /// Load database from its serialized form in json
        /// </summary>
        public static EmulatedDatabase FromJson(JsonObject json, string name)
        {
            var database = new EmulatedDatabase(name);

            database.players.UnionWith(
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
                database.entities.Add(e.id, e);

            database.entityOwnerships.AddRange(
                json["entityOwnerships"]
                    .AsJsonArray
                    .Select(
                        x => new Tuple<string, string>(
                            x.AsJsonObject["entityId"],
                            x.AsJsonObject["playerId"]
                        )
                    )
            );

            return database;
        }

        /// <summary>
        /// Enumerate players inside the database
        /// </summary>
        public IEnumerable<PlayerRecord> EnumeratePlayers()
        {
            return players;
        }

        /// <summary>
        /// Enumerate all entities that belong to a single player
        /// </summary>
        public IEnumerable<RawEntity> EnumeratePlayerEntities(string playerId)
        {
            return entities.Values.Where(e => e.ownerIds.Count == 1 && e.ownerIds.First() == playerId);
        }

        /// <summary>
        /// Enumerate all entities that belong to at least two players
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RawEntity> EnumerateSharedEntities()
        {
            return entities.Values.Where(e => e.ownerIds.Count >= 2);
        }

        /// <summary>
        /// Enumerate all entities that belong to at least two players
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RawEntity> EnumerateGameEntities()
        {
            return entities.Values.Where(e => e.ownerIds.Count == 0);
        }

        /// <summary>
        /// Adds a new player into the database and returns it's ID
        /// </summary>
        public string AddPlayer(string email)
        {
            var id = Str.Random(16);

            players.Add(new PlayerRecord {
                id = id,
                email = email
            });

            OnChange(this);

            return id;
        }

        /// <summary>
        /// Remove a player by it's id
        /// </summary>
        public bool RemovePlayer(string id)
        {
            PlayerRecord player = players.Where(r => r.id == id).FirstOrDefault();

            if (player == null)
                return false;

            players.Remove(player);

            OnChange(this);
            
            return true;
        }

        /// <summary>
        /// Checks proper emulation state.
        /// Throws exception on failure
        /// </summary>
        private void GuardClientSide()
        {
            if (PreventAccess)
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

            OnChange(this);
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
            
            OnChange(this);

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

            // players have no effect on entity selection
            if (entityIds == null)
            {
                entityIds = new HashSet<string>(entities.Keys);
            }

            // load entities
            IEnumerable<RawEntity> loadedEntities = entityIds.Select(id => LoadEntity(id));

            // remove entities that do not match where clauses
            loadedEntities = loadedEntities.Where(e => WhereClausesMatch(e, query));

            // if exact, remove those owned by too many players
            if (query.requireOwnersExactly)
                loadedEntities = loadedEntities.Where(e => e.ownerIds.Count == query.requiredOwners.Count);

            // take only one
            if (query.takeFirstFound)
                return loadedEntities.Take(1);

            return loadedEntities;
        }

        /// <summary>
        /// Return true if all where clauses match the entity
        /// </summary>
        private bool WhereClausesMatch(RawEntity e, EntityQuery query)
        {
            foreach (var clause in query.whereClauses)
            {
                switch (clause["type"].AsString)
                {
                    case "Basic":
                        var extracted = JsonExtract(e.data, clause["path"]);
                        var value = clause["value"];
                        switch (clause["operator"].AsString)
                        {
                            case "=":
                                if (extracted != value)
                                    return false;
                                break;

                            case "!=":
                            case "<>":
                                if (extracted == value)
                                    return false;
                                break;

                            case ">":
                            case ">=":
                            case "<":
                            case "<=":
                                if (value.IsNumber)
                                {
                                    if (clause["operator"].AsString == ">" && !(extracted.AsNumber > value.AsNumber))
                                        return false;
                                    if (clause["operator"].AsString == ">=" && !(extracted.AsNumber >= value.AsNumber))
                                        return false;
                                    if (clause["operator"].AsString == "<" && !(extracted.AsNumber < value.AsNumber))
                                        return false;
                                    if (clause["operator"].AsString == "<=" && !(extracted.AsNumber <= value.AsNumber))
                                        return false;
                                }
                                else
                                {
                                    int c = extracted.AsString.CompareTo(value.AsString);

                                    if (clause["operator"].AsString == ">" && !(c > 0))
                                        return false;
                                    if (clause["operator"].AsString == ">=" && !(c >= 0))
                                        return false;
                                    if (clause["operator"].AsString == "<" && !(c < 0))
                                        return false;
                                    if (clause["operator"].AsString == "<=" && !(c <= 0))
                                        return false;
                                }
                                break;

                            default:
                                throw new UnisaveException(
                                    $"Unknown where clause operator: " + clause["operator"]
                                );
                        }
                        break;

                    default:
                        throw new UnisaveException(
                            $"Unknown where clause type: " + clause["type"]
                        );
                }
            }

            return true;
        }

        private JsonValue JsonExtract(JsonValue subject, string pathString)
        {
            foreach (string step in pathString.Split(new string[] {"->"}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (subject.IsJsonArray)
                {
                    if (int.TryParse(step, out int index))
                        subject = subject.AsJsonArray[index];
                    else
                        subject = JsonValue.Null;

                    continue;
                }

                if (subject.IsJsonObject)
                {
                    subject = subject.AsJsonObject[step];
                    continue;
                }

                subject = JsonValue.Null;
            }

            return subject;
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
