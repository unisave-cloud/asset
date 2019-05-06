using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Unisave.Framework
{
    /// <summary>
    /// Entity that a player always has exactly once
    /// </summary>
    public class PlayerSingleton : PlayerEntity
    {
        public Player Player => GetPlayers().FirstOrDefault();

        public override IList<Entity> Query(Type entityType, EntityQuery query)
        {
            // query by ID goes through
            if (query.Type == EntityQuery.QueryType.ByID)
                return base.Query(entityType, query);

            // query by player has to be by a single player exactly
            if (query.PlayerIDs.Count != 1)
                throw new UnisaveException(
                    "It does not make sense to request" +
                    "PlayerSingleton by different number of players than 1"
                );

            // singleton has to be matched exactly by one player
            query.MatchPlayersExactly = true;

            IList<Entity> results = base.Query(entityType, query);

            // no result => create new instance instead
            // (singleton has to always exist)
            if (results.Count == 0)
            {
                Entity entity = Entity.CreateInstance(entityType, ParentFrameworkBase);
                entity.AddPlayer(new Player(query.PlayerIDs.First()));
                entity.Save();

                results.Add(entity);
            }

            return results;
        }

        public override void Create()
        {
            IList<Entity> results = base.Query(
                this.GetType(),
                EntityQuery.WithPlayers(new string[] { Player.ID })
            );

            if (results.Count > 0)
                throw new UnisaveException(
                    "Cannot create new PlayerSingleton when one already exists."
                );

            base.Create();
        }

        public override void Save()
        {
            if (GetPlayers().Count() != 1)
                throw new UnisaveException("PlayerSingleton must belong to a single player exactly.");

            base.Save();
        }
    }
}
