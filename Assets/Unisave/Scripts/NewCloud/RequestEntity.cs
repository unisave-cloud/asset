using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unisave.Framework;

namespace Unisave
{
    /// <summary>
    /// Facade for requesting entites from server
    /// Can only be used on client side
    /// </summary>
    public static class RequestEntity
    {
        public static Context OfPlayers(IEnumerable<Player> players)
        {
            if (players == null)
                throw new ArgumentNullException(nameof(players));

            if (players.Any(x => x == null))
                throw new ArgumentNullException(
                    nameof(players),
                    "One of the player instances is null."
                );

            return new Context(players);
        }

        public static Context OfPlayer(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            return new Context(new Player[] { player });
        }

        public class Context
        {
            private HashSet<string> playerIDs;

            public Context(IEnumerable<Player> players)
            {
                playerIDs = new HashSet<string>(players.Select(x => x.ID));
            }

            public void RequestAll<T>(Action<IEnumerable<T>> callback) where T : Entity, new()
            {
                UnisaveCloud.Backend.RequestEntity<T>(
                    EntityQuery.WithPlayers(playerIDs),
                    callback
                );
            }

            public void Request<T>(Action<T> callback) where T : Entity, new()
            {
                RequestAll<T>(entities => {
                    foreach (T entity in entities)
                    {
                        callback(entity);
                        return;
                    }
                    callback(null);
                });
            }
        }
    }
}
