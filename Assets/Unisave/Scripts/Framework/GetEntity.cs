using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unisave.Framework;

namespace Unisave.Framework
{
    /// <summary>
    /// Facade for accessing entites in database
    /// Can only be used on server side
    /// </summary>
    public static class GetEntity
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

            public IEnumerable<T> GetAll<T>() where T : Entity, new()
            {
                return StaticBase.Base.GetEntities<T>(EntityQuery.WithPlayers(playerIDs, false));
            }

            public T Get<T>() where T : Entity, new()
            {
                return GetAll<T>().FirstOrDefault();
            }
        }
    }
}
