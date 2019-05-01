using System;
using System.Collections;
using System.Collections.Generic;
using LightJson;

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

            return entity;
        }
    }
}
