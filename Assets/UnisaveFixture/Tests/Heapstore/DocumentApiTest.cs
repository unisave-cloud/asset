using System;
using System.Collections;
using LightJson;
using NUnit.Framework;
using Unisave;
using Unisave.Facets;
using Unisave.Heapstore;
using Unisave.Heapstore.Backend;
using UnisaveFixture.Backend.Core;
using UnisaveFixture.Backend.Core.Arango;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace UnisaveFixture.Tests.Heapstore
{
    [TestFixture]
    public class DocumentApiTest
    {
        private MonoBehaviour caller;

        [UnitySetUp]
        public IEnumerator SetUp()
            => Asyncize.UnityTest(async () =>
        {
            await caller.CallFacet((FullstackUtilsFacet f) =>
                f.ClearDatabase()
            );
        });

        private class Player
        {
            [SerializeAs("_id")]
            public string id;

            public string name;
            public int score;
        }
        
        
        ///////////////////
        // Get operation //
        ///////////////////
        
        [UnityTest]
        public IEnumerator GetFetchesDocumentOrNull()
            => Asyncize.UnityTest(async () =>
        {
            // no collection --> returns null
            Document document = await caller.Document("players/peter").Get();
            Assert.IsNull(document);
            
            // create collection
            await caller.CallFacet((RawAqlFacet f) =>
                f.CreateCollection("players")
            );
            
            // with collection, missing document --> returns null
            document = await caller.Document("players/peter").Get();
            Assert.IsNull(document);
            
            // create document
            await caller.CallFacet((RawAqlFacet f) =>
                f.Run("INSERT { _key: 'peter', name: 'Peter' } INTO players")
            );
            
            // document present, return that document
            document = await caller.Document("players/peter").Get();
            Assert.IsNotNull(document);
            Assert.AreEqual("Peter", document.Data["name"].AsString);
            Assert.IsFalse(document.Data.Contains("_id"));
            Assert.IsFalse(document.Data.Contains("_key"));
            Assert.IsFalse(document.Data.Contains("_rev"));
            
            // document has metadata
            Assert.AreEqual("players/peter", document.Id);
            Assert.AreEqual("players", document.Collection);
            Assert.AreEqual("peter", document.Key);
        });
        
        [UnityTest]
        public IEnumerator DocumentCanBeConvertedToCustomType()
            => Asyncize.UnityTest(async () =>
        {
            await caller.CallFacet((RawAqlFacet f) =>
                f.CreateCollection("players")
            );
            await caller.CallFacet((RawAqlFacet f) => f.Run(@"
                INSERT { _key: 'peter', name: 'Peter', score: 42 } INTO players
            "));
            
            Document document = await caller
                .Document("players/peter")
                .Get();

            Player player = document.As<Player>();
            
            Assert.AreEqual("Peter", player.name);
            Assert.AreEqual(42, player.score);
            Assert.AreEqual("players/peter", player.id);
            
            player = await caller
                .Document("players/peter")
                .GetAs<Player>();
            
            Assert.AreEqual("Peter", player.name);
            Assert.AreEqual(42, player.score);
            Assert.AreEqual("players/peter", player.id);
        });
        
        [UnityTest]
        public IEnumerator GetFailsOnMissingDocument()
            => Asyncize.UnityTest(async () =>
        {
            await caller.CallFacet((RawAqlFacet f) =>
                f.CreateCollection("players")
            );

            try
            {
                await caller
                    .Document("players/peter")
                    .Get(throwIfMissing: true);

                Assert.Fail("Did not throw!");
            }
            catch (HeapstoreException e)
            {
                // ERROR_DOCUMENT_MISSING
                Assert.AreEqual(1000, e.ErrorNumber);
            }
        });
        
        [UnityTest]
        public IEnumerator GetFailsOnMissingCollection()
            => Asyncize.UnityTest(async () =>
        {
            try
            {
                await caller
                    .Document("players/peter")
                    .Get(throwIfMissing: true);

                Assert.Fail("Did not throw!");
            }
            catch (HeapstoreException e)
            {
                // ERROR_DOCUMENT_MISSING
                Assert.AreEqual(1000, e.ErrorNumber);
            }
        });
        
        
        ///////////////////
        // Set operation //
        ///////////////////

        [UnityTest]
        public IEnumerator SetCreatesDocument()
            => Asyncize.UnityTest(async () =>
        {
            JsonObject dbDocument = await caller.CallFacet((RawAqlFacet f) =>
                f.First("RETURN DOCUMENT('players/peter')")
            );
            Assert.IsNull(dbDocument);
            
            await caller
                .Collection("players")
                .Document("peter")
                .Set(new JsonObject {
                    ["name"] = "Peter"
                });
            
            dbDocument = await caller.CallFacet((RawAqlFacet f) =>
                f.First("RETURN DOCUMENT('players/peter')")
            );
            Assert.AreEqual("Peter", dbDocument["name"].AsString);
            Assert.AreEqual("players/peter", dbDocument["_id"].AsString);
        });
        
        [UnityTest]
        public IEnumerator SetReplacesDocument()
            => Asyncize.UnityTest(async () =>
        {
            await caller.CallFacet((RawAqlFacet f) =>
                f.CreateCollection("players")
            );
            await caller.CallFacet((RawAqlFacet f) => f.Run(@"
                INSERT { _key: 'peter', name: 'Peter', otherStuff: true } INTO players
            "));

            await caller
                .Document("players/peter")
                .Set(new JsonObject {
                    ["name"] = "Peter Second",
                    ["score"] = 42
                });
        
            JsonObject dbDocument = await caller.CallFacet((RawAqlFacet f) =>
                f.First("RETURN DOCUMENT('players/peter')")
            );
            Assert.AreEqual("Peter Second", dbDocument["name"].AsString);
            Assert.AreEqual(42, dbDocument["score"].AsInteger);
            Assert.AreEqual("players/peter", dbDocument["_id"].AsString);
            Assert.IsFalse(dbDocument.Contains("otherStuff"));
        });
        
        [UnityTest]
        public IEnumerator SetAcceptsDocumentInstances()
            => Asyncize.UnityTest(async () =>
        {
            Document sourceDoc = new Document(new JsonObject {
                ["_key"] = "foobar", // should be ignored
                ["_id"] = "players/foobar", // should be ignored
                ["_rev"] = "123456789", // should be ignored
                ["name"] = "Peter",
                ["score"] = 42
            });
            
            Document document = await caller
                .Document("players/peter")
                .Set(sourceDoc);
    
            JsonObject dbDocument = await caller.CallFacet((RawAqlFacet f) =>
                f.First("RETURN DOCUMENT('players/peter')")
            );
            Assert.AreEqual("Peter", dbDocument["name"].AsString);
            Assert.AreEqual(42, dbDocument["score"].AsInteger);
            Assert.AreEqual("players/peter", dbDocument["_id"].AsString);
            Assert.AreEqual(2 + 3, dbDocument.Count);
        });
        
        [UnityTest]
        public IEnumerator SetFailsOnMissingDocument()
            => Asyncize.UnityTest(async () =>
        {
            await caller.CallFacet((RawAqlFacet f) =>
                f.CreateCollection("players")
            );

            try
            {
                await caller
                    .Document("players/peter")
                    .Set(new JsonObject(), throwIfMissing: true);

                Assert.Fail("Did not throw!");
            }
            catch (HeapstoreException e)
            {
                // ERROR_DOCUMENT_MISSING
                Assert.AreEqual(1000, e.ErrorNumber);
            }
        });
        
        [UnityTest]
        public IEnumerator SetFailsOnMissingCollection()
            => Asyncize.UnityTest(async () =>
        {
            try
            {
                await caller
                    .Document("players/peter")
                    .Set(new JsonObject(), throwIfMissing: true);

                Assert.Fail("Did not throw!");
            }
            catch (HeapstoreException e)
            {
                // ERROR_DOCUMENT_MISSING
                Assert.AreEqual(1000, e.ErrorNumber);
            }
        });
        
        
        //////////////////////
        // Update operation //
        //////////////////////
        
        // TODO: UpdateModifiesDocument
        
        // TODO: UpdateCreatesDocument
        
        // TODO: UpdateFailsOnMissingCollection
        
        // TODO: UpdateFailsOnMissingDocument
        
        
        ///////////////////
        // Add operation //
        ///////////////////
        
        [UnityTest]
        public IEnumerator AddCreatesDocument()
            => Asyncize.UnityTest(async () =>
        {
            Document document = await caller
                .Collection("players")
                .Add(new JsonObject {
                    ["name"] = "Peter"
                });
            Assert.IsNotNull(document);
            Assert.AreEqual("Peter", document.Data["name"].AsString);
        
            JsonObject dbDocument = await caller.CallFacet((RawAqlFacet f) =>
                f.First("FOR p IN players RETURN p")
            );
            
            Assert.IsNotNull(dbDocument);
            Assert.AreEqual("Peter", dbDocument["name"].AsString);
            Assert.AreEqual(dbDocument["_id"].AsString, document.Id);
        });
        
        [UnityTest]
        public IEnumerator AddFailsOnMissingCollection()
            => Asyncize.UnityTest(async () =>
        {
            try
            {
                await caller
                    .Collection("players")
                    .Add(new JsonObject {
                        ["name"] = "Peter"
                    }, throwIfMissing: true);

                Assert.Fail("Did not throw!");
            }
            catch (HeapstoreException e)
            {
                // ERROR_COLLECTION_MISSING
                Assert.AreEqual(1001, e.ErrorNumber);
            }
        });
        
        
        //////////////////////
        // Delete operation //
        //////////////////////
        
        // TODO: DeleteRemovesDocument
        
        // TODO: DeleteIgnoresMissingDocument
    }
}