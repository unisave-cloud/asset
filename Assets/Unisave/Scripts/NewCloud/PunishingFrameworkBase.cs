using System;
using System.Collections;
using System.Collections.Generic;
using Unisave.Framework;

namespace Unisave
{
    public class PunishingFrameworkBase : IFrameworkBase
    {
        public IEnumerable<T> GetEntities<T>(EntityQuery query) where T : Entity, new()
        {
            throw new InvalidOperationException(
                "You cannot access server code from the client.\n" +
                "Make sure you are using RequestEntity class and not GetEntity class."
            );
        }
    }
}
