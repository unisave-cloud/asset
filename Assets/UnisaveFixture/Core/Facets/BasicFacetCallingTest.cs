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
    /// This suite is testing that facet methods can be called in the basic
    /// way that is listed on the introductory documentation page.
    /// </summary>
    [TestFixture]
    public class BasicFacetCallingTest : FullstackFixture
    {
        protected override string[] BackendFolders => new string[] {
            "Assets/UnisaveFixture/Core/Facets/Backend"
        };
        
        [UnityTest]
        public IEnumerator ItCallsVoidMethod()
            => Asyncize.UnityTest(async () =>
        {
            // expect: does not throw any exceptions
            await caller.CallFacet(
                (MyFacet f) => f.ReturnsVoid()
            );
        });
        
        [UnityTest]
        public IEnumerator ItSendsBasicTypesToAndFromMethod()
            => Asyncize.UnityTest(async () =>
        {
            string receivedString = await caller.CallFacet(
                (MyFacet f) => f.EchoesString("foo")
            );
            Assert.AreEqual("foo", receivedString);
            
            int receivedInt = await caller.CallFacet(
                (MyFacet f) => f.EchoesInt(42)
            );
            Assert.AreEqual(42, receivedInt);
            
            double receivedDouble = await caller.CallFacet(
                (MyFacet f) => f.EchoesDouble(42.3)
            );
            Assert.AreEqual(42.3, receivedDouble);
        });
        
        [UnityTest]
        public IEnumerator ItSendsJsonToAndFromMethod()
            => Asyncize.UnityTest(async () =>
        {
            var sent = new JsonObject {
                ["foo"] = "bar"
            };
            
            JsonValue received = await caller.CallFacet(
                (MyFacet f) => f.EchoesJsonValue(sent)
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
                    (MyFacet f) => f.ThrowsExceptionWithMessage(message)
                );
            
                Assert.Fail("Method has not thrown an exception.");
            }
            catch (Exception e)
            {
                Assert.AreEqual(message, e.Message);
            }
        });
        
        // NOTE: there used to be PHP in the proxy-stack in the early days,
        // but now the test is kept here just in case
        [UnityTest]
        public IEnumerator EmptyJsonObjectCanBeSentCorrectlyThroughPhp()
            => Asyncize.UnityTest(async () =>
        {
            // empty object shouldn't be converted to an empty array
            JsonValue received = await caller.CallFacet(
                (MyFacet f) => f.EchoesJsonValue(new JsonObject())
            );
            Assert.AreEqual("{}", received.ToString());
            
            // similarly arrays must pass through as well
            received = await caller.CallFacet(
                (MyFacet f) => f.EchoesJsonValue(new JsonArray())
            );
            Assert.AreEqual("[]", received.ToString());
            
            // finally, null should also pass through
            received = await caller.CallFacet(
                (MyFacet f) => f.EchoesJsonValue(JsonValue.Null)
            );
            Assert.AreEqual("null", received.ToString());
        });
    }
}