using System;
using System.Collections;
using System.Collections.Generic;
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
        public string ID { get; private set; }

        public static T FromRawData<T>(string id, HashSet<string> playerIDs, JsonObject data) where T : Entity, new()
        {
            T entity = new T();

            entity.ID = id;

            // HACK
            //((PDE)((Entity)entity)).MotorbikeName = data["MotorbikeName"];

            return entity;
        }

        public void Save()
        {
            UnityEngine.Debug.LogError("Entity.Save is not implemented");
        }

        #region "Querying"

        public static EntityContext OfPlayers(IEnumerable<Player> players)
        {
            return new EntityContext(players);
        }

        public static EntityContext OfPlayer(Player player)
        {
            return new EntityContext(new Player[] { player });
        }

        #endregion
    }
}
