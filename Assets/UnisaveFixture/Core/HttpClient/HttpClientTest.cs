using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using Unisave.Facets;
using Unisave.Testing;
using UnisaveFixture.Core.HttpClient.Backend;
using UnityEngine.TestTools;

namespace UnisaveFixture.Core.HttpClient
{
    [TestFixture]
    public class HttpClientTest : FullstackFixture
    {
        protected override string[] BackendFolders => new string[] {
            "Assets/UnisaveFixture/Core/HttpClient/Backend"
        };
        
        [UnityTest]
        public IEnumerator ItGetsWebsiteSync()
            => Asyncize.UnityTest(async () =>
        {
            // expect: does not throw any exceptions
            await caller.CallFacet(
                (HttpClientFacet f) => f.GetWebsiteSync()
            );
        });
        
        [UnityTest]
        public IEnumerator ItGetsWebsiteAsync()
            => Asyncize.UnityTest(async () =>
        {
            // expect: does not throw any exceptions
            await caller.CallFacet(
                (HttpClientFacet f) => f.GetWebsiteAsync()
            );
        });
        
        [UnityTest]
        public IEnumerator ItAbortsTimedOutRequest()
            => Asyncize.UnityTest(async () =>
        {
            bool timedOut = await caller.CallFacet(
                (HttpClientFacet f) => f.TimeoutRequest()
            );
            Assert.IsTrue(timedOut);
        });
    }
}