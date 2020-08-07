using System;
using NUnit.Framework;
using Unisave.Entities;
using Unisave.Facades;
using Unisave.Testing;

namespace UnisaveFixture.Tests.Core.Entities
{
    public class EntityQueryTest : BackendTestCase
    {
        private class PlayerEntity : Entity
        {
            public string name;
            public DateTime premiumUntil = DateTime.UtcNow;
        }
        
        [Test]
        public void ItCanFilterByDateTime()
        {
            new PlayerEntity {
                name = "HasPremium",
                premiumUntil = DateTime.UtcNow.AddHours(1)
            }.Save();
            
            new PlayerEntity {
                name = "NoPremium",
                premiumUntil = DateTime.UtcNow.AddHours(-1)
            }.Save();

            var results = DB.TakeAll<PlayerEntity>()
                .Filter(p => p.premiumUntil > DateTime.UtcNow)
                .Get();
            
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("HasPremium", results[0].name);
        }

        [Test]
        public void FirstOrCreateWorks()
        {
            var player = DB.TakeAll<PlayerEntity>()
                .Filter(p => p.name == "Bob")
                .First();
            
            Assert.IsNull(player);

            bool creatorCalled = false;
            player = DB.TakeAll<PlayerEntity>()
                .Filter(p => p.name == "Bob")
                .FirstOrCreate(p => {
                    p.name = "Bob";
                    creatorCalled = true;
                });
            
            Assert.IsNotNull(player);
            Assert.AreEqual("Bob", player.name);
            Assert.IsTrue(creatorCalled);
            
            creatorCalled = false;
            player = DB.TakeAll<PlayerEntity>()
                .Filter(p => p.name == "Bob")
                .FirstOrCreate(p => {
                    p.name = "Bob";
                    creatorCalled = true;
                });
            
            Assert.IsNotNull(player);
            Assert.AreEqual("Bob", player.name);
            Assert.IsFalse(creatorCalled);
        }
    }
}