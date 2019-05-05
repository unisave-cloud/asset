using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unisave;

namespace Unisave.Framework
{
    public class EntityContext
    {
        private HashSet<string> playerIDs;

        public EntityContext(IEnumerable<Player> playerIDs)
        {
            this.playerIDs = new HashSet<string>(playerIDs.Select(x => x.ID));
        }

        public void RequestAll<T>(Action<IEnumerable<T>> callback) where T : Entity, new()
        {
            // UnisaveCloud.Backend.RequestEntity<T>(
            //     EntityQuery.WithPlayers(playerIDs),
            //     callback
            // );
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

        public IEnumerable<T> GetAll<T>() where T : Entity, new()
        {
            IEnumerable<T> entities = null;
            RequestAll<T>(e => {
                entities = e;
            });
            return entities;
        }

        public T Get<T>() where T : Entity, new()
        {
            T entity = null;
            Request<T>(e => {
                entity = e;
            });
            return entity;
        }
    }
}
