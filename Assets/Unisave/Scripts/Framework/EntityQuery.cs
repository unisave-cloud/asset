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
            ByPlayersAtLeast,
            ByPlayersExactly
        }

        public QueryType Type { get; private set; }

        public string EntityID { get; private set; }

        public HashSet<string> PlayerIDs { get; private set; }

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
                Type = exactly ? QueryType.ByPlayersExactly : QueryType.ByPlayersAtLeast,
                PlayerIDs = new HashSet<string>(playerIDs)
            };
        }
    }
}
