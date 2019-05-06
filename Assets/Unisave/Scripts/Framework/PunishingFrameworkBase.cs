using System;
using System.Collections;
using System.Collections.Generic;
using Unisave.Framework;
using LightJson;

namespace Unisave.Framework
{
    public class PunishingFrameworkBase : IFrameworkBase
    {
        public IList<Entity> QueryEntities(Type entityType, EntityQuery query)
        {
            throw new InvalidOperationException(
                "You cannot access server code from the client.\n" +
                "Make sure you are using RequestEntity class and not GetEntity class."
            );
        }

        public string CreateEntity(string entityType, ISet<string> playerIDs, JsonObject data)
        {
            throw new InvalidOperationException(
                "You cannot access server code from the client.\n" +
                "Entities can only be created on the server."
            );
        }

        public void SaveEntity(string id, ISet<string> playerIDs, JsonObject data)
        {
            throw new InvalidOperationException(
                "You cannot access server code from the client.\n" +
                "Entities can only be updated on the server."
            );
        }

        public void DeleteEntity(string id)
        {
            throw new InvalidOperationException(
                "You cannot access server code from the client.\n" +
                "Entities can only be deleted on the server."
            );
        }
    }
}
