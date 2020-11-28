using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Core.Broadcasting
{
    public class BroadcastingResiliencyTest
    {
        [UnityTest]
        public IEnumerator ItCallsNetworkConnectivityHooks()
        {
            // OnConnectionLost, OnConnectionRegained
            
            Assert.Fail("Not implemented");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator ItReceivesLostEventsOnReconnection()
        {
            // simulate a drop by disabling SSE and setting lastMessageId
            
            Assert.Fail("Not implemented");
            
            yield return null;
        }
    }
}