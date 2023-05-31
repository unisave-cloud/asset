using System.Collections;
using System.Collections.Generic;
using LightJson;
using NUnit.Framework;
using Unisave.Facades;
using Unisave.Facets;
using Unisave.Heapstore;
using Unisave.Serialization;
using UnisaveFixture.Backend.Core;
using UnisaveFixture.Backend.Core.Arango;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace UnisaveFixture.Tests.Heapstore
{
    [TestFixture]
    public class DocumentsApiTest
    {
        private MonoBehaviour caller;
        
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return OnFacet<FullstackUtilsFacet>.Call(
                nameof(FullstackUtilsFacet.ClearDatabase)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator ItCanCreateDocument()
        {
            // insert stuff
            DocumentReference docRef = caller
                .Collection("players")
                .Document("peter");

            // yield return docRef.Set(new {
            //     name = "Peter"
            // });
            
            yield return docRef.Set(new JsonObject() {
                ["name"] = "Peter"
            });
            
            // TODO: check the returned document snapshot
            
            yield return caller
                .CallFacet((RawAqlFacet f) => f.FirstAsString(
                    "RETURN DOCUMENT('players/peter').name"
                ))
                .Then(result => {
                    Assert.AreEqual("Peter", result);
                });
        }
    }
}