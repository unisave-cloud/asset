using System;
using NUnit.Framework;
using Unisave.Entities;
using Unisave.Facades;
using Unisave.Testing;

namespace UnisaveFixture.Tests.Core.Entities
{
    public class EntityReferenceTest : BackendTestCase
    {
        private class StubPlayerEntity : Entity
        { }
        
        private class StubMatchEntity : Entity
        {
            public EntityReference<StubPlayerEntity> Owner { get; set; }
        }

        private StubPlayerEntity player;
        private StubMatchEntity match;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            player = new StubPlayerEntity();
            match = new StubMatchEntity();
            player.Save();
            match.Save();
        }

        [Test]
        public void ReferenceCanBeStored()
        {
            Assert.IsTrue(match.Owner.IsNull);

            match.Owner = player;
            Assert.AreEqual(player.EntityId, match.Owner.TargetId);
            
            match.Save();
            match.Refresh();
            
            Assert.AreEqual(player.EntityId, match.Owner.TargetId);
        }

        [Test]
        public void ReferenceTargetCanBeFound()
        {
            match.Owner = player;
            match.Save();

            var found = match.Owner.Find();
            
            Assert.False(object.ReferenceEquals(player, found));
            Assert.AreEqual(player.EntityId, found.EntityId);
        }

        [Test]
        public void ReferenceTargetCanBeTested()
        {
            Assert.IsTrue(match.Owner == null);
            Assert.IsTrue(match.Owner.TargetId == null);
            Assert.IsTrue(match.Owner.IsNull);
            
            match.Owner = player;
            
            Assert.IsTrue(match.Owner == player);
            Assert.AreEqual(match.Owner, player);
        }

        [Test]
        public void ReferenceCanBeUsedInEntityQuery()
        {
            match.Owner = player;
            match.Save();

            var results = DB.TakeAll<StubMatchEntity>()
                .Filter(entity => entity.Owner == player)
                .Get();
            
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(match.EntityId, results[0].EntityId);
        }

        [Test]
        public void ItChecksIdValidity()
        {
            Assert.Throws<ArgumentException>(() => {
                new EntityReference<StubPlayerEntity>("foobar");
            });
            
            Assert.Throws<ArgumentException>(() => {
                new EntityReference<StubPlayerEntity>("foobar/asd");
            });
            
            Assert.Throws<ArgumentException>(() => {
                new EntityReference<StubPlayerEntity>(
                    EntityUtils.CollectionPrefix + "Nope/asd"
                );
            });
            
            Assert.DoesNotThrow(() => {
                new EntityReference<StubPlayerEntity>(
                    EntityUtils.CollectionPrefix + "StubPlayerEntity/asd"
                );
            });
        }
    }
}