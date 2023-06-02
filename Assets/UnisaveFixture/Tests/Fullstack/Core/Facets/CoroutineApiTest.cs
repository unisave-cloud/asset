using System.Collections;
using System.Text;
using Unisave;
using Unisave.Facets;
using UnisaveFixture.Backend.Core.FacetCalling;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Fullstack.Core.Facets
{
    public class CoroutineApiTest
    {
        [UnityTest]
        public IEnumerator ItCanPingFacet()
        {
            UnisaveOperation<string> request;

            yield return request = FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.Ping("hello")
            );
            
            Assert.AreEqual("Pong: hello", request.Result);
            Assert.IsTrue(request.IsDone);
        }
        
        [UnityTest]
        public IEnumerator ItCanCallVoidMethod()
        {
            UnisaveOperation request;

            yield return request = FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.EmptyMethod()
            );

            Assert.IsTrue(request.IsDone);
        }

        [UnityTest]
        public IEnumerator ItCanCombineWithCallbacks()
        {
            StringBuilder log = new StringBuilder();

            log.Append("A");
            yield return FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.Ping("hello")
            ).Then(r => {
                log.Append("B");
                Assert.AreEqual("Pong: hello", r);
            });
            log.Append("C");
            
            log.Append("_");
            
            log.Append("a");
            yield return FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.EmptyMethod()
            ).Then(() => {
                log.Append("b");
            });
            log.Append("c");
            
            Assert.AreEqual("ABC_abc", log.ToString());
        }
    }
}