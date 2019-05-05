using System;
using System.Collections;
using System.Collections.Generic;

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
        IEnumerable<T> GetEntities<T>(EntityQuery query) where T : Entity, new();
    }
}
