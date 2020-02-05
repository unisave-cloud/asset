using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Editor.Tests.Database.Support;

namespace Unisave.Editor.Tests.Database
{
    /// <summary>
    /// Tests update of entities
    /// </summary>
    public class UpdateEntityTest : DatabaseTestFixture
    {
        [Test]
        public void EntityDataCanBeUpdated()
        {
            var entity = new RawEntity {
                type = "GameEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            Database.SaveEntity(entity);
            
            // change data
            entity.data.Add("newField", 42);
            
            Database.SaveEntity(entity);

            var row = GetEntityRow(entity.id);
            Assert.AreEqual(42, row["data"].AsJsonObject["newField"].AsInteger);
        }
    }
}