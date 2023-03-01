using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using Unisave.Facets;
using UnisaveFixture.Backend.Core.FacetCalling;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace UnisaveFixture.Tests.Fullstack.Core.Facets
{
    public class CallerPresenceTest
    {
        private class MyBehaviour : MonoBehaviour
        {
            // empty
        }

        private GameObject myGameObject;
        private MyBehaviour myCaller;
        
        /*
         * NOTE: We don't test that coroutines don't finish, because it doesn't
         * make sense. They stop executing because Unity stops advancing them.
         * Therefore we can use them here in these tests to actually wait for
         * the request completion.
         */

        [SetUp]
        public void SetUp()
        {
            myGameObject = new GameObject(
                nameof(CallerPresenceTest),
                typeof(MyBehaviour)
            );
            myCaller = myGameObject.GetComponent<MyBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(myGameObject);
        }
        
        [UnityTest]
        public IEnumerator ItCanPingFacet()
        {
            bool thenCalled = false;
            
            // game object is enabled
            
            yield return myCaller.CallFacet(
                (SwissKnifeFacet f) => f.Ping("hello")
            ).Then(r => {
                thenCalled = true;
            });
            
            Assert.IsTrue(thenCalled);
        }
        
        [UnityTest]
        public IEnumerator ItDoesNotCallThenCallback()
        {
            bool thenCalled = false;
            
            // game object is disabled
            myGameObject.SetActive(false);

            yield return myCaller.CallFacet(
                (SwissKnifeFacet f) => f.Ping("hello")
            ).Then(r => {
                thenCalled = true;
            });
            
            Assert.IsFalse(thenCalled);
        }
        
        [UnityTest]
        public IEnumerator ItDoesNotCallCatchCallback()
        {
            bool catchCalled = false;
            
            // game object is disabled
            myGameObject.SetActive(false);

            yield return myCaller.CallFacet(
                (SwissKnifeFacet f) => f.ThrowInvalidOperationException()
            ).Catch(r => {
                catchCalled = true;
            });
            
            Assert.IsFalse(catchCalled);
        }
        
        [UnityTest]
        public IEnumerator ItDoesNotCompleteTask()
        {
            // game object is disabled
            myGameObject.SetActive(false);

            var request = myCaller.CallFacet(
                (SwissKnifeFacet f) => f.Ping("hello")
            );
            var task = request.Async();
            
            yield return request;
            
            Assert.IsTrue(request.IsDone); // request finished
            Assert.IsFalse(task.IsCompleted); // but task did not
        }
        
        [UnityTest]
        public IEnumerator ItDoesNotFaultTask()
        {
            // game object is disabled
            myGameObject.SetActive(false);

            var request = myCaller.CallFacet(
                (SwissKnifeFacet f) => f.ThrowInvalidOperationException()
            );
            var task = request.Async();
            
            yield return request;
            
            Assert.IsTrue(request.IsDone); // request finished
            Assert.IsFalse(task.IsFaulted); // but task did not
        }
    }
}