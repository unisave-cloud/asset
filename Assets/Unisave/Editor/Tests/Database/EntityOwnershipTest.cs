using System.Collections.Generic;
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
        public void OwnerCanBeAdded()
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
        public void OwnerCanBeRemoved()
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
        public void OwnersCanBeQueried()
        {
            ISet<string> ownerIds = new HashSet<string>();

            for (int i = 0; i < 100; i++)
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
            
            foreach (string queriedOwner in queriedOwners)
            {
                // queried player is indeed one of the owners
                Assert.IsTrue(ownerIds.Contains(queriedOwner));
                ownerIds.Remove(queriedOwner);
            }
            
            // and the query returned all the owners
            Assert.IsEmpty(ownerIds);
            
            // also check that we can stop iteration during iteration
            foreach (string queriedOwner in queriedOwners)
                break;
        }
    }
}