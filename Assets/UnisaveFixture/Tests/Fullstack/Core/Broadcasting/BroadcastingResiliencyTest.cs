using System.Collections;
using NUnit.Framework;
using Unisave.Facades;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Core.Broadcasting
{
    public class BroadcastingResiliencyTest
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // if there is an app already, dispose it so that
            // we are sure we will make a fresh connection
            if (ClientFacade.HasApp)
            {
                var app = ClientFacade.ClientApp;
                app.Dispose();
            }
            
            yield return null;
        }
        
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