using System.Collections.Generic;
using System.Linq;
using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Editor.Tests.Database.Support;
using UnityEngine;

namespace Unisave.Editor.Tests.Database
{
    public class QueryEntityTest : DatabaseTestFixture
    {
        public RawEntity CreateGameEntity(string type = "GameEntity")
        {
            var entity = new RawEntity {
                type = type,
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            Database.SaveEntity(entity);
            return entity;
        }
        
        public RawEntity CreatePlayerEntity(
            string type = "PlayerEntity",
            string owner = null
        )
        {
            owner = owner ?? CreatePlayer();
            var entity = new RawEntity {
                type = type,
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            entity.ownerIds.Add(owner);
            Database.SaveEntity(entity);
            return entity;
        }
        
        //////////////////
        // Test methods //
        ////////////////// 
        
        [Test]
        public void NonExactGameEntities()
        {
            CreatePlayerEntity();
            string player = CreatePlayer();
            string a = CreatePlayerEntity("TargetEntity", player).id;
            
            CreateGameEntity();
            string b = CreateGameEntity("TargetEntity").id;

            if (Mode == TestMode.DatabaseProxy)
                mySqlTestFixture.CreateEntityInAnotherDatabase("TargetEntity");

            var q = Database.QueryEntities(new EntityQuery {
                entityType = "TargetEntity",
                requireOwnersExactly = false
            }).ToList();
            
            Assert.IsTrue(q.Any(x => x.id == a));
            Assert.IsTrue(q.Any(x => x.id == b));
            Assert.AreEqual(2, q.Count);

            // owners should be loaded
            if (q[0].id == a)
                Assert.IsTrue(q[0].ownerIds.Contains(player));
            else
                Assert.IsTrue(q[1].ownerIds.Contains(player));
        }
        
        [Test]
        public void ExactGameEntities()
        {
            CreatePlayerEntity();
            string a = CreatePlayerEntity("TargetEntity").id;
            
            CreateGameEntity();
            string b = CreateGameEntity("TargetEntity").id;

            if (Mode == TestMode.DatabaseProxy)
                mySqlTestFixture.CreateEntityInAnotherDatabase("TargetEntity");

            var q = Database.QueryEntities(new EntityQuery {
                entityType = "TargetEntity",
                requireOwnersExactly = true
            }).ToList();

            Assert.IsFalse(q.Any(x => x.id == a));
            Assert.IsTrue(q.Any(x => x.id == b));
            Assert.AreEqual(1, q.Count);
        }
    }
}