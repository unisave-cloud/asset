using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Editor.Tests.Database.Support;

namespace Unisave.Editor.Tests.Database
{
    /// <summary>
    /// Tests that an entity can be loaded
    /// </summary>
    public class LoadEntityTest : DatabaseTestFixture
    {
        [Test]
        public void GameEntityCanBeLoaded()
        {
            var entity = new RawEntity {
                type = "GameEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            Database.SaveEntity(entity);

            var loadedEntity = Database.LoadEntity(entity.id);
            
            Assert.AreEqual(entity.id, loadedEntity.id);
            Assert.AreEqual(entity.type, loadedEntity.type);
            Assert.AreEqual(entity.data.ToString(), loadedEntity.data.ToString());
            Assert.AreEqual(entity.createdAt, loadedEntity.createdAt);
            Assert.AreEqual(entity.updatedAt, loadedEntity.updatedAt);
        }

        [Test]
        public void CannotLoadEntityFromAnotherDatabase()
        {
            // MySQL only test
            if (Mode != TestMode.DatabaseProxy)
            {
                Assert.IsTrue(true);
                return;
            }

            string entityId = mySqlTestFixture.CreateEntityInAnotherDatabase();

            Assert.IsNull(
                Database.LoadEntity(entityId)
            );
        }
    }
}