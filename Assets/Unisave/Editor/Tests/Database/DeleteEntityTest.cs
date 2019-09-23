using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Editor.Tests.Database.Support;

namespace Unisave.Editor.Tests.Database
{
    public class DeleteEntityTest : DatabaseTestFixture
    {
        [Test]
        public void GameEntityCanBeDeleted()
        {
            var entity = new RawEntity {
                type = "GameEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            Database.SaveEntity(entity);

            AssertDatabaseHasEntity(entity.id);

            Assert.IsTrue(Database.DeleteEntity(entity.id));
            
            AssertDatabaseMissingEntity(entity.id);
        }
        
        [Test]
        public void PlayerEntityCanBeDeleted()
        {
            var owner = CreatePlayer();
            var entity = new RawEntity {
                type = "GameEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            entity.ownerIds.Add(owner);
            Database.SaveEntity(entity);

            AssertDatabaseHasEntity(entity.id);

            Assert.IsTrue(Database.DeleteEntity(entity.id));
            
            AssertDatabaseMissingEntity(entity.id);
        }
        
        [Test]
        public void NonExistingEntityCanBeDeleted()
        {
            const string id = "foobar";
            
            AssertDatabaseMissingEntity(id);

            Assert.IsFalse(Database.DeleteEntity(id));
            
            AssertDatabaseMissingEntity(id);
        }
    }
}