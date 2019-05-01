using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Framework;

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

        public LocalDatabase()
        {
            players.Add(new PlayerRecord {
                id = "LOCAL_ID",
                email = "local"
            });

            entities.Add(new EntityRecord {
                id = "ABC123",
                type = "PDE",
                playerIDs = new HashSet<string>(new string[] {"LOCAL_ID"}),
                data = new JsonObject().Add("message", "hello world!")
            });
        }

        public IEnumerable<T> RunEntityQuery<T>(EntityQuery query) where T : Entity, new()
        {
            var entitiesOfType = from e in entities where e.type == typeof(T).Name select e;
            IEnumerable<EntityRecord> records = null;

            if (query.Type == EntityQuery.QueryType.ByID)
                records = from e in entitiesOfType where e.id == query.EntityID select e;

            if (query.Type == EntityQuery.QueryType.ByPlayersAtLeast)
                records = from e in entitiesOfType where query.PlayerIDs.IsSubsetOf(e.playerIDs) select e;

            if (query.Type == EntityQuery.QueryType.ByPlayersExactly)
                records = from e in entitiesOfType where query.PlayerIDs.SetEquals(e.playerIDs) select e;

            if (records == null)
                throw new ArgumentException("Provided query type is not supported.");

            return from r in records select Entity.FromRawData<T>(r.id, r.playerIDs, r.data);
        }
    }
}
