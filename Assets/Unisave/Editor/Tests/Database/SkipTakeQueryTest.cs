using System.Linq;
using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Database.Query;
using Unisave.Editor.Tests.Database.Support;

namespace Unisave.Editor.Tests.Database
{
    public class SkipTakeQueryTest : DatabaseTestFixture
    {
        [Test]
        [Ignore("Not implemented yet")]
        public void ItTakesOneResult()
        {
            var a = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("Name", "John")
            };
            Database.SaveEntity(a);
            
            var b = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("Name", "Steve")
            };
            Database.SaveEntity(b);
            
            var q = Database.QueryEntities(new EntityQuery {
                entityType = "MyEntity",
                take = 1
            }).ToList();
            
            Assert.AreEqual(1, q.Count);
            Assert.IsTrue(a.id == q[0].id || b.id == q[0].id);
        }
    }
}