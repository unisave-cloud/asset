using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LightJson;
using System.Linq;

namespace Unisave.Framework
{
    /// <summary>
    /// Entity holds data about your game
    /// Entity can belong to a set of players
    /// </summary>
    public class Entity
    {
        protected IFrameworkBase ParentFrameworkBase { get; private set; } = StaticBase.Base;

        public string ID { get; private set; }

        private ISet<string> playerIDs = new HashSet<string>();

        public string EntityType => EntityUtils.GetEntityType(this.GetType());

        public virtual bool CanHaveEmptyPlayers => false;

        public static T CreateInstance<T>(IFrameworkBase parentBase) where T : Entity, new()
        {
            T instance = new T();
            instance.ParentFrameworkBase = parentBase;
            return instance;
        }

        public static T CreateInstance<T>(
            IFrameworkBase parentBase, string id, IEnumerable<string> playerIDs, JsonObject data
        ) where T : Entity, new()
        {
            T entity = CreateInstance<T>(parentBase);

            entity.ID = id;
            entity.playerIDs = new HashSet<string>(playerIDs);
            
            EntityUtils.DistributeData(entity, data);

            return entity;
        }

        public virtual IList<T> Query<T>(EntityQuery query) where T : Entity, new()
        {
            return ParentFrameworkBase.QueryEntities<T>(query);
        }

        public virtual void Create()
        {
            if (ID != null)
                throw new UnisaveException("Cannot create an entity, when it already exists.");

            ID = ParentFrameworkBase.CreateEntity(
                EntityType,
                playerIDs,
                EntityUtils.CollectData(this)
            );
        }

        public virtual void Save()
        {
            if (ID == null)
            {
                Create();
                return;
            }

            if (playerIDs.Count == 0 && !CanHaveEmptyPlayers)
            {
                Delete();
                return;
            }
            
            ParentFrameworkBase.SaveEntity(
                ID,
                playerIDs,
                EntityUtils.CollectData(this)
            );
        }

        public virtual void Delete()
        {
            if (ID == null)
                throw new UnisaveException("Cannot delete entity that does not exist yet.");

            ParentFrameworkBase.DeleteEntity(ID);
        }

        public virtual void AddPlayer(Player player)
        {
            playerIDs.Add(player.ID);
        }

        public virtual void RemovePlayer(Player player)
        {
            playerIDs.Remove(player.ID);
        }
    }
}
