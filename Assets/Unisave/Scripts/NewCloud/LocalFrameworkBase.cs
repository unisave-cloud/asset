using System;
using System.Collections;
using System.Collections.Generic;
using Unisave.Framework;

namespace Unisave
{
    public class LocalFrameworkBase : IFrameworkBase
    {
        private LocalDatabase database;

        public LocalFrameworkBase(LocalDatabase database)
        {
            this.database = database;
        }

        public IEnumerable<T> GetEntities<T>(EntityQuery query) where T : Entity, new()
        {
            return database.RunEntityQuery<T>(query);
        }
    }
}
