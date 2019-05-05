using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unisave.Framework;
using LightJson;

namespace Unisave
{
    public class LocalFrameworkBase : IFrameworkBase
    {
        private LocalDatabase database;

        public LocalFrameworkBase(LocalDatabase database)
        {
            this.database = database;
        }

        public IList<T> QueryEntities<T>(EntityQuery query) where T : Entity, new()
        {
            return database.RunEntityQuery<T>(query)
                .Select(x => Entity.CreateInstance<T>(this, x.id, x.playerIDs, x.data))
                .ToList();
        }

        public string CreateEntity(string entityType, ISet<string> playerIDs, JsonObject data)
        {
            string id = database.GenerateNewEntityId();
            
            database.entities.Add(new LocalDatabase.EntityRecord {
                id = id,
                type = entityType,
                playerIDs = new HashSet<string>(playerIDs),
                data = data
            });

            database.Save();

            return id;
        }

        public void SaveEntity(string id, ISet<string> playerIDs, JsonObject data)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var record = database.entities.Where(x => x.id == id).First();
            record.playerIDs = new HashSet<string>(playerIDs);
            record.data = data;
            database.Save();
        }

        public void DeleteEntity(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            database.entities.RemoveAll(x => x.id == id);
        }
    }
}
