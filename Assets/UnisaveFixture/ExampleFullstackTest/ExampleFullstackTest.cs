using System.Collections;
using NUnit.Framework;
using Unisave.Facets;
using Unisave.Testing;
using UnisaveFixture.ExampleFullstackTest.Backend;
using UnityEngine.TestTools;

namespace UnisaveFixture.ExampleFullstackTest
{
    /// <summary>
    /// This test fixture demonstrates what can be done with a fullstack fixture
    /// </summary>
    [TestFixture]
    public class ExampleFullstackTest : FullstackFixture
    {
        /// <summary>
        /// You must list the backend folders to be uploaded to the server
        /// </summary>
        protected override string[] BackendFolders => new string[] {
            "Assets/UnisaveFixture/ExampleFullstackTest/Backend"
        };
        
        /// <summary>
        /// You can call facets as usual, directly using the caller field,
        /// or indirectly by loading a scene and running its scripts.
        /// In fact, anything can be used, just like when building a real game.
        /// </summary>
        [UnityTest]
        public IEnumerator ItCallsFacetMethod()
            => Asyncize.UnityTest(async () =>
        {
            string response = await caller.CallFacet(
                (ExampleFacet f) => f.Echo("Hello world!")
            );
        
            Assert.AreEqual(
                "Hello world!",
                response
            );
        });
    }
}