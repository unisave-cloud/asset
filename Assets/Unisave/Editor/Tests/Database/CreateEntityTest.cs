using System;
using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Editor.Tests.Database.Support;
using UnityEngine;

namespace Unisave.Editor.Tests.Database
{
    /// <summary>
    /// Tests entity creation
    /// </summary>
    public class CreateEntityTest : DatabaseTestFixture
    {
        [Test]
        public void GameEntityCanBeCreated()
        {
            var entity = new RawEntity {
                type = "GameEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            
            Database.SaveEntity(entity);

            Assert.NotNull(entity.id);
            Assert.IsTrue((DateTime.UtcNow - entity.createdAt).TotalSeconds < 5);
            Assert.IsTrue((DateTime.UtcNow - entity.updatedAt).TotalSeconds < 5);

            JsonObject row = GetEntityRow(entity.id);
            
            AssertDatabaseIdMatches(row["database_id"].AsString);
            
            Assert.AreEqual(entity.id, row["id"].AsString);
            Assert.AreEqual(
                entity.createdAt,
                DateTime.Parse(row["created_at"].AsString)
            );
            Assert.AreEqual(
                entity.updatedAt,
                DateTime.Parse(row["updated_at"].AsString)
            );
        }

        [Test]
        public void PlayerEntityCanBeCreated()
        {
            string john = CreatePlayer();
            string peter = CreatePlayer();
            
            var entity = new RawEntity {
                type = "PlayerEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            entity.ownerIds.Add(john);
            
            Database.SaveEntity(entity);

            AssertPlayerOwns(john, entity.id);
            AssertPlayerNotOwns(peter, entity.id);
        }
        
        [Test]
        public void SharedEntityCanBeCreated()
        {
            string john = CreatePlayer();
            string simon = CreatePlayer();
            string frank = CreatePlayer();
            
            string peter = CreatePlayer();
            
            var entity = new RawEntity {
                type = "SharedEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            entity.ownerIds.Add(john);
            entity.ownerIds.Add(simon);
            entity.ownerIds.Add(frank);
            
            Database.SaveEntity(entity);

            AssertPlayerOwns(john, entity.id);
            AssertPlayerOwns(simon, entity.id);
            AssertPlayerOwns(frank, entity.id);
            
            AssertPlayerNotOwns(peter, entity.id);
        }
    }
}