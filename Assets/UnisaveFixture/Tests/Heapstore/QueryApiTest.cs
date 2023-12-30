using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightJson;
using NUnit.Framework;
using Unisave;
using Unisave.Facets;
using Unisave.Heapstore;
using Unisave.Serialization;
using Unisave.Testing;
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
        
        private class Item
        {
            [SerializeAs("_id")]
            public string id;

            public string name;
            public int price;
        }

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
        
        [UnityTest]
        public IEnumerator FirstReturnsTheFirstDocument()
            => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 1);

            Document document = await caller
                .Collection("items")
                .First();
        
            Assert.AreEqual(1, document.Data["price"].AsInteger);
        });
        
        [UnityTest]
        public IEnumerator FirstReturnsNullWhenNoDocuments()
            => Asyncize.UnityTest(async () =>
        {
            Document document = await caller
                .Collection("items")
                .First();
    
            Assert.IsNull(document);
        });
        
        [UnityTest]
        public IEnumerator FirstConvertsToCustomType()
            => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 1);
            
            Item item = await caller
                .Collection("items")
                .FirstAs<Item>();

            Assert.AreEqual("item_1", item.name);
            Assert.AreEqual(1, item.price);
        });
        
        [UnityTest]
        public IEnumerator FirstConvertsToNullWhenNoDocuments()
            => Asyncize.UnityTest(async () =>
        {
            Item item = await caller
                .Collection("items")
                .FirstAs<Item>();

            Assert.IsNull(item);
        });
        
        [UnityTest]
        public IEnumerator GetConvertsToCustomType()
            => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 10);
            
            List<Item> items = await caller
                .Collection("items")
                .GetAs<Item>();

            items.Sort((a, b) => a.price.CompareTo(b.price));
            
            Assert.AreEqual(10, items.Count);
            for (int i = 1; i <= 10; i++)
            {
                Assert.AreEqual("item_" + i, items[i-1].name);
                Assert.AreEqual(i, items[i-1].price);
            }
        });
        
        
        ///////////////////
        // Filter clause //
        ///////////////////
        
        [UnityTest]
        public IEnumerator FilterSelectsByEquality()
            => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 10);
            
            List<Document> documents = await caller
                .Collection("items")
                .Filter("price", "==", 5)
                .Get();
    
            Assert.AreEqual(1, documents.Count);
            Assert.AreEqual("item_5", documents[0].Data["name"].AsString);
        });
        
        [UnityTest]
        public IEnumerator FilterByMultipleClauses()
            => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 10);
        
            List<Item> items = await caller
                .Collection("items")
                .Filter("price", ">=", 2)
                .Filter("price", "<=", 5)
                .GetAs<Item>();

            items.Sort((a, b) => a.price.CompareTo(b.price));
            
            Assert.AreEqual(4, items.Count);
            for (int i = 2; i <= 5; i++)
            {
                Assert.AreEqual("item_" + i, items[i-2].name);
                Assert.AreEqual(i, items[i-2].price);
            }
        });
        
        
        /////////////////
        // Sort clause //
        /////////////////
        
        [UnityTest]
        public IEnumerator SortAscending()
            => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 10);
    
            List<Item> items = await caller
                .Collection("items")
                .Sort("price", "ASC")
                .GetAs<Item>();

            Assert.AreEqual(10, items.Count);
            for (int i = 1; i <= 10; i++)
                Assert.AreEqual(i, items[i-1].price);
        });
        
        [UnityTest]
        public IEnumerator SortDescending()
            => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 10);

            List<Item> items = await caller
                .Collection("items")
                .Sort("price", "DESC")
                .GetAs<Item>();

            Assert.AreEqual(10, items.Count);
            for (int i = 1; i <= 10; i++)
                Assert.AreEqual(11 - i, items[i-1].price);
        });
        
        [UnityTest]
        public IEnumerator SortMultiple()
            => Asyncize.UnityTest(async () =>
        {
            await caller.CallFacet(
                (RawAqlFacet f) => f.CreateCollection("items")
            );
            await caller.CallFacet((RawAqlFacet f) => f.Run(@"
                FOR doc IN [
                    { name: 'A', price: 1 },
                    { name: 'B', price: 1 },
                    { name: 'C', price: 2 },
                    { name: 'D', price: 2 }
                ] INSERT doc INTO items
            "));

            List<Item> items = await caller
                .Collection("items")
                .Sort(("price", "DESC"), ("name", "ASC"))
                .GetAs<Item>();

            string result = string.Concat(items.Select(i => i.name));
            Assert.AreEqual("CDAB", result);
        });
        
        
        //////////////////
        // Limit clause //
        //////////////////
        
        [UnityTest]
        public IEnumerator LimitTake()
            => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 10);

            List<Item> items = await caller
                .Collection("items")
                .Sort("price", "ASC")
                .Limit(take: 3)
                .GetAs<Item>();

            Assert.AreEqual(3, items.Count);
            for (int i = 1; i <= 3; i++)
                Assert.AreEqual(i, items[i-1].price);
        });
        
        [UnityTest]
        public IEnumerator LimitSkipTake()
            => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 10);

            List<Item> items = await caller
                .Collection("items")
                .Sort("price", "ASC")
                .Limit(skip: 2, take: 3)
                .GetAs<Item>();

            Assert.AreEqual(3, items.Count);
            for (int i = 1; i <= 3; i++)
                Assert.AreEqual(i + 2, items[i-1].price);
        });
        
        [UnityTest]
        public IEnumerator LimitSkipTakeFirst()
            => Asyncize.UnityTest(async () =>
        {
            await GenerateItems(count: 10);

            Item item = await caller
                .Collection("items")
                .Sort("price", "ASC")
                .Limit(skip: 2, take: 6)
                .FirstAs<Item>();

            Assert.AreEqual(3, item.price);
        });
    }
}