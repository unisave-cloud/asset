using System.Collections;
using LightJson;
using NUnit.Framework;
using Unisave.Facets;
using Unisave.Heapstore;
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
                f.Run("INSERT { _key: 'peter' name: 'Peter' } INTO players")
            );
            
            // document present, return that document
            document = await caller.Document("players/peter").Get();
            Assert.IsNotNull(document);
            Assert.AreEqual("Peter", document.Data["name"].AsString);
            Assert.AreEqual("players/peter", document.Data["_id"].AsString);
            
            // document has metadata
            Assert.AreEqual("players/peter", document.Id);
            Assert.AreEqual("players", document.Collection);
            Assert.AreEqual("peter", document.Key);
        });
        
        // TODO: document can be converted to custom type

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
        
        // TODO: SetReplacesDocument
        
        // TODO: SetFailsOnMissingDocument
        
        
        //////////////////////
        // Update operation //
        //////////////////////
        
        // TODO: UpdateModifiesDocument
        
        // TODO: UpdateCreatesDocument
        
        // TODO: UpdateFailsOnMissingDocument
        
        
        ///////////////////
        // Add operation //
        ///////////////////
        
        // TODO: AddCreatesDocument
        
        
        //////////////////////
        // Delete operation //
        //////////////////////
        
        // TODO: DeleteRemovesDocument
        
        // TODO: DeleteIgnoresMissingDocument
        
        // TODO: NOTE: Set and Update might want to fail on missing documents
    }
}