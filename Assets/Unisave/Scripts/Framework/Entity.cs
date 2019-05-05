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
        public string ID { get; private set; }

        public static T FromRawData<T>(string id, HashSet<string> playerIDs, JsonObject data) where T : Entity, new()
        {
            T entity = new T();

            entity.ID = id;

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            foreach (PropertyInfo pi in typeof(T).GetProperties(flags))
            {
                // both accessors needed
                if (!pi.CanRead || !pi.CanWrite)
                    continue;

                // both accessors non static
                if (pi.GetSetMethod().IsStatic || pi.GetGetMethod().IsStatic)
                    continue;

                // load the value
                pi.GetSetMethod().Invoke(entity, new object[] {
                    Unisave.Serialization.Loader.Load(data[pi.Name], pi.PropertyType)
                });
            }

            return entity;
        }

        public void Save()
        {
            UnityEngine.Debug.LogError("Entity.Save is not implemented");

            // create if no ID
            // carried IFrameworkBase.saveEntity(this)
        }
    }
}
