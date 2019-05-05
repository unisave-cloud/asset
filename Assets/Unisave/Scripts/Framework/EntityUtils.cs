using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LightJson;

namespace Unisave.Framework
{
    public static class EntityUtils
    {
        /// <summary>
        /// Returns type of a given entity, that is used for storing the entity inside the database
        /// </summary>
        public static string GetEntityType<T>() where T : Entity
        {
            return GetEntityType(typeof(T));
        }

        public static string GetEntityType(Type entityType)
        {
            if (!typeof(Entity).IsAssignableFrom(entityType))
                throw new ArgumentException("Provided type does not inherit from Entity");

            return entityType.Name;
        }

        /// <summary>
        /// Distributes data from a JSON object into an entity instance
        /// </summary>
        public static void DistributeData(Entity entity, JsonObject data)
        {
            foreach (PropertyInfo pi in IterateProperties(entity.GetType()))
            {
                pi.GetSetMethod().Invoke(entity, new object[] {
                    Unisave.Serialization.Loader.Load(data[pi.Name], pi.PropertyType)
                });
            }
        }

        /// <summary>
        /// Collects data from an entity instance into a JSON object
        /// </summary>
        public static JsonObject CollectData(Entity entity)
        {
            JsonObject data = new JsonObject();

            foreach (PropertyInfo pi in IterateProperties(entity.GetType()))
            {
                data.Add(pi.Name, Unisave.Serialization.Saver.Save(
                    pi.GetGetMethod().Invoke(entity, new object[] {})
                ));
            }

            return data;
        }

        private static IEnumerable<PropertyInfo> IterateProperties(Type entityType)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            foreach (PropertyInfo pi in entityType.GetProperties(flags))
            {
                // both accessors needed
                if (!pi.CanRead || !pi.CanWrite)
                    continue;

                // both accessors non static
                if (pi.GetSetMethod().IsStatic || pi.GetGetMethod().IsStatic)
                    continue;

                yield return pi;
            }
        }
    }
}
