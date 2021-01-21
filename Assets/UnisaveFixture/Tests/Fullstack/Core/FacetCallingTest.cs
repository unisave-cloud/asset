using System.Collections;
using LightJson;
using NUnit.Framework;
using Unisave.Facades;
using UnisaveFixture.Backend.Core.FacetCalling;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Core
{
    public class FacetCallingTest
    {
        [UnityTest]
        public IEnumerator VoidFacetMethodCanBeCalled()
        {
            yield return OnFacet<FcFacet>.Call(
                nameof(FcFacet.VoidFacet)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator VoidFacetMethodCanThrowException()
        {
            string message = "Lorem ipsum dolor sit...";
            
            yield return OnFacet<FcFacet>.Call(
                nameof(FcFacet.VoidFacetThatThrows),
                message
            )
                .Then(() => {
                    Assert.Fail("Method has not thrown an exception.");
                })
                .Catch(e => {
                    Assert.AreEqual(message, e.Message);
                })
                .AsCoroutine();
        }

        [UnityTest]
        public IEnumerator JsonCanBeSentCorrectlyThroughPhp()
        {
            yield return OnFacet<FcFacet>.Call<JsonValue>(
                    nameof(FcFacet.JsonTest),
                    new JsonObject()
                )
                .Then(json => {
                    Assert.AreEqual("{}", json.ToString());
                })
                .AsCoroutine();
            
            yield return OnFacet<FcFacet>.Call<JsonValue>(
                    nameof(FcFacet.JsonTest),
                    new JsonArray()
                )
                .Then(json => {
                    Assert.AreEqual("[]", json.ToString());
                })
                .AsCoroutine();
            
            yield return OnFacet<FcFacet>.Call<JsonValue>(
                    nameof(FcFacet.JsonTest),
                    new object[] { null } // "null" would be an object[], not object
                )
                .Then(json => {
                    Assert.AreEqual("null", json.ToString());
                })
                .AsCoroutine();
        }
    }
}