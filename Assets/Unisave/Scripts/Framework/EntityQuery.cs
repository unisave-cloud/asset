using System;
using System.Collections;
using System.Collections.Generic;

namespace Unisave.Framework
{
    /// <summary>
    /// Represents a query of some entities
    /// </summary>
    public class EntityQuery
    {
        public enum QueryType
        {
            ByID,
            ByPlayers
        }

        public QueryType Type { get; private set; }

        public string EntityID { get; private set; }

        public HashSet<string> PlayerIDs { get; private set; }

        public bool MatchPlayersExactly { get; set; } = false;

        public static EntityQuery WithId(string entityID)
        {
            return new EntityQuery {
                Type = QueryType.ByID,
                EntityID = entityID
            };
        }

        public static EntityQuery WithPlayers(IEnumerable<string> playerIDs, bool exactly = false)
        {
            return new EntityQuery {
                Type = QueryType.ByPlayers,
                PlayerIDs = new HashSet<string>(playerIDs),
                MatchPlayersExactly = exactly
            };
        }
    }
}
