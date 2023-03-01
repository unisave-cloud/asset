using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Unisave.Facets;
using UnisaveFixture.Backend.Core.FacetCalling;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Fullstack.Core.Facets
{
    public class AsyncAwaitApiTest
    {
        // Unity tests do not support async-await, so here's the workaround:
        // https://answers.unity.com/questions/1597151/async-unit-test-in-test-runner.html
        private static IEnumerator AwaitTaskInCoroutine(Task task)
        {
            while (!task.IsCompleted) // breaks when faulted, we dont' throw
                yield return null;
        }
        
        [UnityTest]
        public IEnumerator ItCanPingFacet()
        {
            Task<string> task = FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.Ping("hello")
            ).Async();

            yield return AwaitTaskInCoroutine(task);
            
            Assert.AreEqual("Pong: hello", task.Result);
        }
        
        [UnityTest]
        public IEnumerator ItCanCallVoidMethod()
        {
            Task task = FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.EmptyMethod()
            ).Async();

            yield return AwaitTaskInCoroutine(task);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsFaulted);
        }
        
        [UnityTest]
        public IEnumerator ItCanCombineWithCallbacks()
        {
            StringBuilder log = new StringBuilder();

            log.Append("A");
            yield return AwaitTaskInCoroutine(
                FacetClient.CallFacet(
                    (SwissKnifeFacet f) => f.Ping("hello")
                ).Then(r => {
                    log.Append("B");
                    Assert.AreEqual("Pong: hello", r);
                }).Async()
            );
            log.Append("C");
            
            log.Append("_");
            
            log.Append("a");
            yield return AwaitTaskInCoroutine(
                FacetClient.CallFacet(
                    (SwissKnifeFacet f) => f.EmptyMethod()
                ).Then(() => {
                    log.Append("b");
                }).Async()
            );
            log.Append("c");
            
            Assert.AreEqual("ABC_abc", log.ToString());
        }

        [UnityTest]
        public IEnumerator ExceptionCanBeAwaitedAndItWontGetLogged()
        {
            Task task = FacetClient.CallFacet(
                (SwissKnifeFacet f) => f.ThrowInvalidOperationException()
            ).Async();

            yield return AwaitTaskInCoroutine(task);

            Assert.AreEqual(
                typeof(InvalidOperationException),
                task.Exception?.GetBaseException().GetType()
            );
            Assert.IsTrue(task.IsFaulted);
            
            // nothing gets logged, because the creation of the task
            // disables the default log-to-console-if-not-catching behaviour
            LogAssert.NoUnexpectedReceived();
        }
    }
}