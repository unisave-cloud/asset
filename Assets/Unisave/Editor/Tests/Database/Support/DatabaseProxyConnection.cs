using System.Collections.Generic;
using Unisave.Database;

namespace Unisave.Editor.Tests.Database.Support
{
    /// <summary>
    /// Connection to the database proxy
    /// Implements the Framework database interface
    /// </summary>
    public class DatabaseProxyConnection : IDatabase
    {
        public void SaveEntity(RawEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public RawEntity LoadEntity(string id)
        {
            throw new System.NotImplementedException();
        }

        public bool DeleteEntity(string id)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<RawEntity> QueryEntities(string entityType, EntityQuery query)
        {
            throw new System.NotImplementedException();
        }
    }
}