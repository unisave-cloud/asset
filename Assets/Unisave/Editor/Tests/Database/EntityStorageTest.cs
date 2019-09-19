using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Editor.Tests.Database.Support;

namespace Unisave.Editor.Tests.Database
{
    public class EntityStorageTest : DatabaseTestFixture
    {
        [Test]
        public void GameEntityCanBeCreated()
        {
            var entity = new RawEntity {
                type = "SomeEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            
            Database.SaveEntity(entity);
            
            AssertDatabaseHasEntity(entity.id);
        }
    }
}