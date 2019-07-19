using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unisave;
using Unisave.Serialization;
using Unisave.Utils;

namespace Unisave.Database
{
    /// <summary>
    /// Emulated analogue to the Unisave.Database.UnisaveDatabase
    /// </summary>
    public class EmulatedDatabase : IDatabase
    {
        /// <summary>
        /// This is controlled by the EmulatedFacetCaller
        /// We don't want developer to access database from client side,
        /// but we want it, when we simulate facet execution.
        /// </summary>
        public bool IsEmulatingFacetCall { get; set; }

        /// <summary>
        /// List of all entities
        /// </summary>
        private Dictionary<string, RawEntity> entities = new Dictionary<string, RawEntity>();

        /// <summary>
        /// Pairs of [ entity | player ]
        /// </summary>
        private List<Tuple<string, string>> entityOwnerships = new List<Tuple<string, string>>();

        /// <summary>
        /// Load database content from player prefs
        /// </summary>
        public void LoadDatabase()
        {

        }

        /// <summary>
        /// Save database to player prefs
        /// </summary>
        public void SaveDatabase()
        {

        }

        /// <summary>
        /// Checks proper emulation state.
        /// Throws exception on failure
        /// </summary>
        private void GuardClientSide()
        {
            if (!IsEmulatingFacetCall)
                FakeDatabase.NotifyDeveloper();
        }

        /// <inheritdoc/>
        public void SaveEntity(RawEntity entity)
        {
            GuardClientSide();

            if (entity.id == null)
                InsertEntity(entity);
            else
                UpdateEntity(entity);
        }

        private void InsertEntity(RawEntity entity)
        {
            entity.id = Str.Random(16);
            entity.updatedAt = entity.createdAt = DateTime.UtcNow;

            entities.Add(entity.id, RawEntity.FromJson(entity.ToJson()));

            AddOwners(entity.id, entity.ownerIds);
        }

        private void UpdateEntity(RawEntity entity)
        {
            entity.updatedAt = DateTime.UtcNow;

            entities[entity.id] = RawEntity.FromJson(entity.ToJson());

            RemoveAllOwners(entity.id);
            AddOwners(entity.id, entity.ownerIds);
        }

        /// <inheritdoc/>
        public RawEntity LoadEntity(string id)
        {
            GuardClientSide();

            if (!entities.ContainsKey(id))
                return null;

            var entity = RawEntity.FromJson(entities[id].ToJson());

            entity.ownerIds = new HashSet<string>(GetEntityOwners(id));

            return entity;
        }

        /// <inheritdoc/>
        public bool DeleteEntity(string id)
        {
            GuardClientSide();

            RemoveAllOwners(id);

            if (!entities.ContainsKey(id))
                return false;

            entities.Remove(id);
            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<RawEntity> QueryEntities(string entityType, EntityQuery query)
        {
            /*
                This implementation is really not the best possible, but I didn't want to waste
                time by overly optimizing a database, that is going to have hundreds of items at most.
                (remember, this is the emulated one, not the real one)
             */

            GuardClientSide();

            // build a set of entities that are owned by all the required players
            HashSet<string> entityIds = null; // null means the entire universe

            foreach (UnisavePlayer player in query.requiredOwners)
            {
                var entityIdsOwnedByThisPlayer = entityOwnerships
                    .Where(t => t.Item2 == player.Id)
                    .Select(t => t.Item1);

                var playerEntityIds = entities
                    .Where(p => p.Value.type == entityType)
                    .Where(p => entityIdsOwnedByThisPlayer.Contains(p.Value.id))
                    .Select(p => p.Value.id);

                if (entityIds == null)
                    entityIds = new HashSet<string>(playerEntityIds);
                else
                    entityIds.IntersectWith(playerEntityIds);
            }

            // load entities
            IEnumerable<RawEntity> loadedEntities = entityIds.Select(id => LoadEntity(id));

            // if exact, remove those owned by too many players
            if (query.requireOwnersExactly)
                loadedEntities = loadedEntities.Where(e => e.ownerIds.Count == query.requiredOwners.Count);

            // take only one
            if (query.takeFirstFound)
                return loadedEntities.Take(1);

            return loadedEntities;
        }

        /// <summary>
        /// Returns a set of owners of a given entity
        /// </summary>
        private IEnumerable<string> GetEntityOwners(string entityId)
        {
            return entityOwnerships.Where(t => t.Item1 == entityId).Select(t => t.Item2);
        }

        /// <summary>
        /// Removes all owners of an entity
        /// </summary>
        private void RemoveAllOwners(string entityId)
        {
            entityOwnerships.RemoveAll(t => t.Item1 == entityId);
        }

        /// <summary>
        /// Adds given owners to the entity.
        /// Assumes all owners are new
        /// </summary>
        private void AddOwners(string entityId, ISet<string> ownerIds)
        {
            foreach (string ownerId in ownerIds)
                entityOwnerships.Add(new Tuple<string, string>(entityId, ownerId));
        }
    }
}
