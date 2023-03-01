using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unisave.Facets;
using UnisaveFixture.Backend.Core.FacetCalling;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Fullstack.Core.Facets
{
    public class CallbackApiTest
    {
        [UnityTest]
        public IEnumerator ItCanPingFacet()
        {
            string response = null;

            FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.Ping("hello")
            ).Then(r => {
                response = r;
            });

            yield return new WaitUntil(() => response != null);
            
            Assert.AreEqual("Pong: hello", response);
        }
        
        [UnityTest]
        public IEnumerator ItCanCallVoidMethod()
        {
            bool done = false;

            FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.EmptyMethod()
            ).Then(() => {
                done = true;
            });

            yield return new WaitUntil(() => done);
        }
        
        [UnityTest]
        public IEnumerator ExceptionGetsLoggedIfNotCaught()
        {
            LogAssert.Expect(
                LogType.Exception,
                new Regex(
                    @"InvalidOperationException",
                    RegexOptions.Multiline
                )
            );
            
            var request = FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.ThrowInvalidOperationException()
            );

            yield return new WaitUntil(() => request.IsDone);
        }
        
        [UnityTest]
        public IEnumerator ExceptionCanBeCaught()
        {
            Exception exception = null;

            var request = FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.ThrowInvalidOperationException()
            ).Catch(e => {
                exception = e;
            });

            yield return new WaitUntil(() => request.IsDone);
            
            LogAssert.NoUnexpectedReceived();
            
            Assert.AreEqual(typeof(InvalidOperationException), exception?.GetType());
        }

        [UnityTest]
        public IEnumerator ExceptionInThenCallbackGetsLogged()
        {
            LogAssert.Expect(
                LogType.Exception,
                new Regex(
                    @"This should be logged!",
                    RegexOptions.Multiline
                )
            );
            
            var request = FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.EmptyMethod()
            ).Then(() => {
                throw new Exception("This should be logged!");
            });

            yield return new WaitUntil(() => request.IsDone);
        }
        
        [UnityTest]
        public IEnumerator ExceptionInCatchCallbackGetsLogged()
        {
            LogAssert.Expect(
                LogType.Exception,
                new Regex(
                    @"This should be logged!",
                    RegexOptions.Multiline
                )
            );
            
            var request = FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.ThrowInvalidOperationException()
            ).Catch(e => {
                throw new Exception("This should be logged!");
            });

            yield return new WaitUntil(() => request.IsDone);
        }
    }
}