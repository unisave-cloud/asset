using System;
using System.Collections;
using LightJson;
using NUnit.Framework;
using Unisave.Facets;
using Unisave.Testing;
using UnisaveFixture.Core.Facets.Backend;
using UnityEngine.TestTools;

namespace UnisaveFixture.Core.Facets
{
    /// <summary>
    /// Analogous to the BasicFacetCallingTest, only tests facet methods
    /// that are asynchronous.
    /// </summary>
    [TestFixture]
    public class BasicAsyncFacetCallingTest : FullstackFixture
    {
        protected override string[] BackendFolders => new string[] {
            "Assets/UnisaveFixture/Core/Facets/Backend"
        };
        
        [UnityTest]
        public IEnumerator ItCallsTaskMethod()
            => Asyncize.UnityTest(async () =>
        {
            // expect: does not throw any exceptions
            await caller.CallFacet(
                (MyAsyncFacet f) => f.ReturnsTask()
            );
        });
        
        [UnityTest]
        public IEnumerator ItSendsStringsToAndFromMethod()
            => Asyncize.UnityTest(async () =>
        {
            string receivedString = await caller.CallFacet(
                (MyAsyncFacet f) => f.EchoesStringAsync("foo")
            );
            Assert.AreEqual("foo", receivedString);
        });
        
        [UnityTest]
        public IEnumerator ItSendsJsonToAndFromMethod()
            => Asyncize.UnityTest(async () =>
        {
            var sent = new JsonObject {
                ["foo"] = "bar"
            };
        
            JsonValue received = await caller.CallFacet(
                (MyAsyncFacet f) => f.EchoesJsonValueAsync(sent)
            );
        
            Assert.AreEqual(sent.ToString(), received.ToString());
        });
        
        [UnityTest]
        public IEnumerator ItTransportsExceptions()
            => Asyncize.UnityTest(async () =>
        {
            string message = "Lorem ipsum dolor sit...";

            try
            {
                await caller.CallFacet(
                    (MyAsyncFacet f) => f.ThrowsExceptionWithMessageAsync(message)
                );
        
                Assert.Fail("Method has not thrown an exception.");
            }
            catch (Exception e)
            {
                Assert.AreEqual(message, e.Message);
            }
        });
        
        [UnityTest]
        public IEnumerator ItRefusesToCallAsyncVoidMethods()
            => Asyncize.UnityTest(async () =>
        {
            try
            {
                await caller.CallFacet(
                    (MyAsyncFacet f) => f.AsyncVoidCannotBeCalled()
                );
    
                Assert.Fail("Method has not thrown an exception.");
            }
            catch (Exception e)
            {
                // Unisave.Facets.MethodSearchException: Method 'UnisaveFixture.
                // Core.Facets.Backend.MyAsyncFacet.AsyncVoidCannotBeCalled'
                // cannot be declared as 'async void'. Use 'async Task' instead.
                StringAssert.Contains(
                    "cannot be declared as 'async void'. Use 'async Task'",
                    e.Message
                );
            }
        });
    }
}