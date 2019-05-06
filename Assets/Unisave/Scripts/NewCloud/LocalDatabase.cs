using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using LightJson.Serialization;
using Unisave.Framework;
using UnityEngine;

namespace Unisave
{
    /// <summary>
    /// Stores data for the LocalBackend class
    /// </summary>
    public class LocalDatabase
    {
        public class PlayerRecord
        {
            public string id;
            public string email;
        }

        public class EntityRecord
        {
            public string id;
            public string type;
            public HashSet<string> playerIDs = new HashSet<string>();
            public JsonObject data;
        }

        public List<PlayerRecord> players = new List<PlayerRecord>();
        public List<EntityRecord> entities = new List<EntityRecord>();
        private string name;

        public LocalDatabase(string name)
        {
            this.name = name;
        }

        public string GenerateNewEntityId()
        {
            var r = new System.Random();
            return "ENTITY_" + entities.Count + "_" + r.Next().ToString();
        }

        public void Load()
        {
            players.Clear();
            entities.Clear();

            string json = PlayerPrefs.GetString("unisave-local-database:" + name, "");
            
            try
            {
                JsonObject obj = JsonReader.Parse(json).AsJsonObject;

                foreach (JsonObject p in obj["players"].AsJsonArray)
                {
                    players.Add(new PlayerRecord {
                        id = p["id"].AsString,
                        email = p["email"].AsString
                    });
                }

                foreach (JsonObject e in obj["entities"].AsJsonArray)
                {
                    entities.Add(new EntityRecord {
                        id = e["id"].AsString,
                        type = e["type"].AsString,
                        playerIDs = new HashSet<string>(e["playerIDs"].AsJsonArray.Select(x => x.AsString)),
                        data = e["data"].AsJsonObject
                    });
                }
            }
            catch (JsonParseException)
            {
                // nothing
            }

            CreateLocalPlayerIfNeeded();
        }

        public void Save()
        {
            JsonArray ps = new JsonArray();
            foreach (PlayerRecord p in players)
            {
                ps.Add(new JsonObject()
                    .Add("id", p.id)
                    .Add("email", p.email)
                );
            }

            JsonArray es = new JsonArray();
            foreach (EntityRecord e in entities)
            {
                var pids = new JsonArray();

                foreach (string p in e.playerIDs)
                    pids.Add(p);

                es.Add(new JsonObject()
                    .Add("id", e.id)
                    .Add("type", e.type)
                    .Add("playerIDs", pids)
                    .Add("data", e.data)
                );
            }

            JsonObject obj = new JsonObject()
                .Add("players", ps)
                .Add("entities", es);

            PlayerPrefs.SetString("unisave-local-database:" + name, obj.ToString());
            PlayerPrefs.Save();
        }

        private void CreateLocalPlayerIfNeeded()
        {
            const string email = "local";

            if (players.Where(x => x.email == email).Count() == 0)
            {
                players.Add(new PlayerRecord {
                    id = "ID_LOCAL",
                    email = email
                });
            }
        }

        public IEnumerable<EntityRecord> RunEntityQuery(string entityType, EntityQuery query)
        {
            var entitiesOfType = from e in entities where e.type == entityType select e;
            IEnumerable<EntityRecord> records = null;

            if (query.Type == EntityQuery.QueryType.ByID)
                records = from e in entitiesOfType where e.id == query.EntityID select e;

            if (query.Type == EntityQuery.QueryType.ByPlayers && !query.MatchPlayersExactly)
                records = from e in entitiesOfType where query.PlayerIDs.IsSubsetOf(e.playerIDs) select e;

            if (query.Type == EntityQuery.QueryType.ByPlayers && query.MatchPlayersExactly)
                records = from e in entitiesOfType where query.PlayerIDs.SetEquals(e.playerIDs) select e;

            if (records == null)
                throw new ArgumentException("Provided query type is not supported.");

            return records;
        }
    }
}
