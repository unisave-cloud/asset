using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LightJson;
using NUnit.Framework;
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
    public class QueryApiTest
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

        private async Task GenerateItems(int count)
        {
            await caller.CallFacet(
                (RawAqlFacet f) => f.CreateCollection("items")
            );
            
            await caller.CallFacet((RawAqlFacet f) => f.Run(@"
                FOR i IN 1.." + count + @"
                    INSERT { name: CONCAT('item_', i), price: i } INTO items
            "));
        }

        [UnityTest]
        public IEnumerator EntireCollectionCanBeListed()
            => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 10);

            List<Document> documents = await caller
                .Collection("items")
                .Get();
            
            // sort by price so that the asserts check all of them correctly
            documents.Sort((a, b) =>
                a.Data["price"].AsInteger.CompareTo(b.Data["price"].AsInteger)
            );
            
            Assert.AreEqual(10, documents.Count);
            for (int i = 1; i <= 10; i++)
            {
                Assert.AreEqual(
                    "item_" + i,
                    documents[i-1].Data["name"].AsString
                );
                Assert.AreEqual(i, documents[i-1].Data["price"].AsInteger);
            }
        });
        
        [UnityTest]
        public IEnumerator QueryResultIsLimitedTo1K()
        => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 2000);
        
            List<Document> documents = await caller
                .Collection("items")
                .Get();
            
            Assert.AreEqual(1_000, documents.Count);
        });
        
        [UnityTest]
        public IEnumerator QueryingMissingCollectionReturnsEmptyList()
            => Asyncize.UnityTest(async () =>
        {
            List<Document> documents = await caller
                .Collection("items")
                .Get();
        
            Assert.AreEqual(0, documents.Count);
        });
    }
}