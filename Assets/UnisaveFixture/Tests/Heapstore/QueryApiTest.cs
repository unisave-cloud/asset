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

        [UnityTest]
        public IEnumerator SetCanCreateDocument()
            => Asyncize.UnityTest(async () =>
        {
            //
        });
    }
}