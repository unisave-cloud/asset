using System.Collections.Generic;
using System.Linq;
using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Database.Query;
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
        
        public RawEntity CreateSharedEntity(
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
            entity.ownerIds.Add(CreatePlayer());
            entity.ownerIds.Add(CreatePlayer());
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

        [Test]
        public void NonExactPlayerEntities()
        {
            CreatePlayerEntity();
            string player = CreatePlayer();
            string a = CreatePlayerEntity("TargetEntity", player).id;
            string b = CreateSharedEntity("TargetEntity", player).id;
            
            string c = CreatePlayerEntity("TargetEntity").id;
            string d = CreateSharedEntity("TargetEntity").id;
            
            var q = Database.QueryEntities(new EntityQuery {
                entityType = "TargetEntity",
                requireOwnersExactly = false,
                requiredOwners = new HashSet<UnisavePlayer>(
                    new UnisavePlayer[] {
                        new UnisavePlayer(player)
                    }
                )
            }).ToList();
            
            Assert.AreEqual(2, q.Count);
            Assert.IsTrue(q.Any(x => x.id == a));
            Assert.IsTrue(q.Any(x => x.id == b));
            Assert.IsFalse(q.Any(x => x.id == c));
            Assert.IsFalse(q.Any(x => x.id == d));
        }
        
        [Test]
        public void ExactPlayerEntities()
        {
            CreatePlayerEntity();
            string player = CreatePlayer();
            string a = CreatePlayerEntity("TargetEntity", player).id;
            string b = CreateSharedEntity("TargetEntity", player).id;
            
            string c = CreatePlayerEntity("TargetEntity").id;
            string d = CreateSharedEntity("TargetEntity").id;
            
            var q = Database.QueryEntities(new EntityQuery {
                entityType = "TargetEntity",
                requireOwnersExactly = true,
                requiredOwners = new HashSet<UnisavePlayer>(
                    new UnisavePlayer[] {
                        new UnisavePlayer(player)
                    }
                )
            }).ToList();
            
            Assert.AreEqual(1, q.Count);
            Assert.IsTrue(q.Any(x => x.id == a));
            Assert.IsFalse(q.Any(x => x.id == b));
            Assert.IsFalse(q.Any(x => x.id == c));
            Assert.IsFalse(q.Any(x => x.id == d));
        }
        
        [Test]
        public void NonExactSharedEntities()
        {
            CreateGameEntity();
            CreatePlayerEntity();
            CreateSharedEntity("TargetEntity");
            RawEntity target = CreateSharedEntity("TargetEntity");
            
            CreateSharedEntity("TargetEntity", target.ownerIds.First());
            
            var q = Database.QueryEntities(new EntityQuery {
                entityType = "TargetEntity",
                requireOwnersExactly = false,
                requiredOwners = new HashSet<UnisavePlayer>(
                    new UnisavePlayer[] {
                        new UnisavePlayer(target.ownerIds.First()),
                        new UnisavePlayer(target.ownerIds.Skip(1).First())
                    }
                )
            }).ToList();
            
            Assert.AreEqual(1, q.Count);
            Assert.IsTrue(q[0].id == target.id);
        }
        
        [Test]
        public void ExactSharedEntities()
        {
            CreateGameEntity();
            CreatePlayerEntity();
            CreateSharedEntity("TargetEntity");
            RawEntity target = CreateSharedEntity("TargetEntity");
            CreateSharedEntity("TargetEntity", target.ownerIds.First());
            
            var q = Database.QueryEntities(new EntityQuery {
                entityType = "TargetEntity",
                requireOwnersExactly = true,
                requiredOwners = new HashSet<UnisavePlayer>(
                    new UnisavePlayer[] {
                        new UnisavePlayer(target.ownerIds.First()),
                        new UnisavePlayer(target.ownerIds.Skip(1).First()),
                        new UnisavePlayer(target.ownerIds.Skip(2).First())
                    }
                )
            }).ToList();
            
            Assert.AreEqual(1, q.Count);
            Assert.AreEqual(target.id, q[0].id);
        }
    }
}