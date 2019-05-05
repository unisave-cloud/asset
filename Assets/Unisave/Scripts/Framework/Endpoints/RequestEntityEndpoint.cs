using System;
using System.Collections;
using System.Collections.Generic;
using Unisave.Framework;

namespace Unisave.Framework.Endpoints
{
    public class RequestEntityEndpoint
    {
        private IFrameworkBase frameworkBase;

        public RequestEntityEndpoint(IFrameworkBase frameworkBase)
        {
            this.frameworkBase = frameworkBase;
        }

        public IList<T> RequestEntity<T>(EntityQuery query) where T : Entity, new()
        {
            return Entity.CreateInstance<T>(frameworkBase).Query<T>(query);
        }
    }
}
