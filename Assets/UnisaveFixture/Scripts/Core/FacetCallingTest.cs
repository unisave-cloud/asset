using System.Collections;
using NUnit.Framework;
using Unisave.Facades;
using UnisaveFixture.Backend.Core.FacetCalling;
using UnityEngine.TestTools;

namespace UnisaveFixture.Core
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