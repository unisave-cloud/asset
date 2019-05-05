using System;
using System.Collections;
using System.Collections.Generic;
using LightJson;

namespace Unisave.Framework
{
    /// <summary>
    /// Represents framework connection to the database
    /// </summary>
    public interface IFrameworkBase
    {
        /////////////////////
        // Player database //
        /////////////////////
        
        // ...

        /////////////////////
        // Entity database //
        /////////////////////

        /// <summary>
        /// Get entities of type T satisfying the provided query
        /// </summary>
        IList<T> QueryEntities<T>(EntityQuery query) where T : Entity, new();

        /// <summary>
        /// Create new entity instance and return it's ID
        /// </summary>
        string CreateEntity(string entityType, ISet<string> playerIDs, JsonObject data);

        /// <summary>
        /// Save entity data
        /// </summary>
        void SaveEntity(string id, ISet<string> playerIDs, JsonObject data);

        /// <summary>
        /// Delete an entity
        /// </summary>
        void DeleteEntity(string id);
    }
}
