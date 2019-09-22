using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Editor.Tests.Database.Support;

namespace Unisave.Editor.Tests.Database
{
    /// <summary>
    /// Tests addition and removal of entity owners
    /// And other things related to ownership
    /// </summary>
    public class EntityOwnershipTest : DatabaseTestFixture
    {
        [Test]
        public void OwnerCanBeAddedToCompleteKnowledgeEntity()
        {
            string stablePlayer = CreatePlayer();
            
            var entity = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            entity.ownerIds.Add(stablePlayer);
            Database.SaveEntity(entity);
            
            // add owner
            string newPlayer = CreatePlayer();
            entity.ownerIds.Add(newPlayer);
            
            AssertPlayerOwns(stablePlayer, entity.id);
            AssertPlayerNotOwns(newPlayer, entity.id);
            
            Database.SaveEntity(entity);
            
            // assert
            AssertPlayerOwns(newPlayer, entity.id);
            AssertPlayerOwns(stablePlayer, entity.id);
        }

        [Test]
        public void OwnerCanBeRemovedFromCompleteKnowledgeEntity()
        {
            string stablePlayer = CreatePlayer();
            string removedPlayer = CreatePlayer();
            
            var entity = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            entity.ownerIds.Add(stablePlayer);
            entity.ownerIds.Add(removedPlayer);
            Database.SaveEntity(entity);
            
            // remove owner
            entity.ownerIds.Remove(removedPlayer);
            
            AssertPlayerOwns(stablePlayer, entity.id);
            AssertPlayerOwns(removedPlayer, entity.id);
            
            Database.SaveEntity(entity);
            
            // assert
            AssertPlayerOwns(stablePlayer, entity.id);
            AssertPlayerNotOwns(removedPlayer, entity.id);
        }

        [Test]
        public void OwnerCanBeAddedToIncompleteKnowledgeEntity()
        {
            string stablePlayer = CreatePlayer();
            var entity = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            entity.ownerIds.Add(stablePlayer);
            Database.SaveEntity(entity);
            
            // make the knowledge incomplete
            entity.ownerIds.IsComplete = false;
            
            // add owner
            string newPlayer = CreatePlayer();
            entity.ownerIds.Add(newPlayer);
            Database.SaveEntity(entity);
            
            // assert
            AssertPlayerOwns(newPlayer, entity.id);
            AssertPlayerOwns(stablePlayer, entity.id);
        }

        [Test]
        public void OwnerCanBeRemovedFromIncompleteKnowledgeEntity()
        {
            string stablePlayer = CreatePlayer();
            string removedPlayer = CreatePlayer();
            
            var entity = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            entity.ownerIds.Add(stablePlayer);
            entity.ownerIds.Add(removedPlayer);
            Database.SaveEntity(entity);
            
            // make the entity knowledge incomplete
            entity.ownerIds.IsComplete = false;
            
            // remove owner
            entity.ownerIds.Remove(removedPlayer);
            
            AssertPlayerOwns(stablePlayer, entity.id);
            AssertPlayerOwns(removedPlayer, entity.id);
            
            Database.SaveEntity(entity);
            
            // assert
            AssertPlayerOwns(stablePlayer, entity.id);
            AssertPlayerNotOwns(removedPlayer, entity.id);
        }
        
        [Test]
        public void OwnersCanBeQueried()
        {
            ISet<string> ownerIds = new HashSet<string>();

            for (var i = 0; i < 100; i++)
                ownerIds.Add(CreatePlayer());
            
            var entity = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };

            foreach (string owner in ownerIds)
                entity.ownerIds.Add(owner);

            Database.SaveEntity(entity);
            
            // now I can query and iterate over the owners
            IEnumerable<string> queriedOwners = Database.GetEntityOwners(
                entity.id
            );
            
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (string queriedOwner in queriedOwners)
            {
                // queried player is indeed one of the owners
                Assert.IsTrue(ownerIds.Contains(queriedOwner));
                ownerIds.Remove(queriedOwner);
            }
            
            // and the query returned all the owners
            Assert.IsEmpty(ownerIds);
            
            // also check that we can stop iteration during iteration
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (string queriedOwner in queriedOwners)
                break;
        }

        [Test]
        public void GameEntityLoadsWithCompleteOwnerKnowledge()
        {
            var entity = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            Database.SaveEntity(entity);

            var loadedEntity = Database.LoadEntity(entity.id);
            
            Assert.IsTrue(loadedEntity.ownerIds.IsComplete);
            Assert.IsEmpty(loadedEntity.ownerIds.GetKnownOwners());
            Assert.AreEqual(0, loadedEntity.ownerIds.Count);
        }
        
        [Test]
        public void PlayerEntityLoadsWithCompleteOwnerKnowledge()
        {
            string player = CreatePlayer();
            
            var entity = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            entity.ownerIds.Add(player);
            Database.SaveEntity(entity);

            var loadedEntity = Database.LoadEntity(entity.id);
            
            Assert.IsTrue(loadedEntity.ownerIds.IsComplete);
            Assert.IsNotEmpty(loadedEntity.ownerIds.GetKnownOwners());
            Assert.AreEqual(1, loadedEntity.ownerIds.Count);
            Assert.AreEqual(player, loadedEntity.ownerIds.First());
        }
        
        [Test]
        public void SharedEntityLoadsWithIncompleteOwnerKnowledge()
        {
            var players = new HashSet<string>();

            for (var i = 0; i < 100; i++)
                players.Add(CreatePlayer());
            
            var entity = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            entity.ownerIds.AddRange(players);
            Database.SaveEntity(entity);

            var loadedEntity = Database.LoadEntity(entity.id);
            
            Assert.IsFalse(loadedEntity.ownerIds.IsComplete);
            
            // each of the owners that we know of, are real owners indeed 
            foreach (string k in loadedEntity.ownerIds.GetKnownOwners())
                Assert.IsTrue(players.Contains(k));
        }

        [Test]
        public void ExistingPlayerCanBeAddedAndNothingBreaks()
        {
            string owner = CreatePlayer();
            var entity = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            entity.ownerIds.Add(owner);
            Database.SaveEntity(entity);
            
            // nothing breaks
            entity.ownerIds.Add(owner);
            Database.SaveEntity(entity);
        }

        [Test]
        public void NonExistingPlayerCanBeRemovedAndNothingBreaks()
        {
            string owner = CreatePlayer();
            
            var entity = new RawEntity {
                type = "MyEntity",
                data = new JsonObject()
                    .Add("foo", "bar")
            };
            Database.SaveEntity(entity);
            
            // nothing breaks
            entity.ownerIds.Remove(owner);
            Database.SaveEntity(entity);
        }
    }
}