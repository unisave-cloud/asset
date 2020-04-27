using System.Collections;
using AcceptanceTests.Backend.FacetCalling;
using NUnit.Framework;
using Unisave.Facades;
using UnityEngine.TestTools;

namespace AcceptanceTests
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
    }
}