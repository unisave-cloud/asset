using System.Collections.Generic;
using System.Diagnostics;
using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Editor.Tests.Database.Support;

namespace Unisave.Editor.Tests.Database
{
    public class DatabaseBenchmarks : DatabaseTestFixture
    {
        [Test]
        [Ignore("Benchmarks are not needed for typical test run.")]
        public void PlayerEntityCreateLoadAndUpdateBenchmark()
        {
            string owner = CreatePlayer();
            List<RawEntity> entities = new List<RawEntity>();

            // create 1000 entities
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
                var entity = new RawEntity {
                    type = "MyEntity",
                    data = new JsonObject()
                        .Add("foo", "bar")
                };
                entity.ownerIds.Add(owner);
                Database.SaveEntity(entity);
                entities.Add(entity);
            }
            sw.Stop();

            UnityEngine.Debug.Log("Insertion time (ms): " + sw.ElapsedMilliseconds);
            
            // load 1000 entities
            sw = new Stopwatch();
            sw.Start();
            foreach (var entity in entities)
                Database.LoadEntity(entity.id);
            sw.Stop();

            UnityEngine.Debug.Log("Retrieval time (ms): " + sw.ElapsedMilliseconds);
            
            // (do the entity update)
            foreach (var entity in entities)
                entity.data["foo"] = "bar,bar";
            
            // update 1000 entities
            sw = new Stopwatch();
            sw.Start();
            foreach (var entity in entities)
                Database.SaveEntity(entity);
            sw.Stop();

            UnityEngine.Debug.Log("Update time (ms): " + sw.ElapsedMilliseconds);
        }
    }
}