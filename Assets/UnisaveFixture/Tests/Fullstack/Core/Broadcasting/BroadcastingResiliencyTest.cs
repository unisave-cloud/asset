using System.Collections;
using NUnit.Framework;
using Unisave.Broadcasting;
using Unisave.Broadcasting.Sse;
using Unisave.Facades;
using UnisaveFixture.Backend.Core.Broadcasting;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Core.Broadcasting
{
    public class BroadcastingResiliencyTest
    {
        private MyBroadcastingClient client;
        
        private void CreateClient()
        {
            var go = new GameObject("MyBroadcastingClient");
            go.SetActive(false);
            client = go.AddComponent<MyBroadcastingClient>();
            client.channelParameter = "foo";
            client.sendMessageAfterSubscribing = false;
            go.SetActive(true);
        }
        
        private IEnumerator WaitForClientToSettle()
        {
            while (!client.hasSettled)
                yield return null;
        }
        
        private IEnumerator WaitForMessages(int count, float timeout = 5f)
        {
            float start = Time.time;

            while (client.receivedMessages.Count < count)
            {
                if (Time.time - start > timeout)
                {
                    Debug.LogError("Waited for messages but none arrived.");
                    Assert.Fail();
                }

                yield return null;
            }
        }
        
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
            CreateClient();
            yield return WaitForClientToSettle();
            
            Assert.False(client.onConnectionLostCalled);
            Assert.False(client.onConnectionRegainedCalled);

            // check connection is made
            yield return OnFacet<BroadcastingFacet>.Call(
                nameof(BroadcastingFacet.SendMyMessage),
                "foo",
                "Hello world!"
            ).AsCoroutine();
            yield return WaitForMessages(1);
            
            var tunnel = ClientFacade.ClientApp.Services
                .Resolve<ClientBroadcastingManager>()
                .Tunnel;
            
            // drop connection

            ((DefaultSseSocket)tunnel.Socket).state.RetryMilliseconds = 500; // retry in 0.5s
            ((DefaultSseSocket)tunnel.Socket).RunningRequest.Abort();

            while (((DefaultSseSocket)tunnel.Socket).RunningRequest != null)
                yield return null;

            Assert.True(client.onConnectionLostCalled);
            Assert.False(client.onConnectionRegainedCalled);
            client.onConnectionLostCalled = false;
            
            // wait for reconnect (by sending and waiting for a message)
            yield return OnFacet<BroadcastingFacet>.Call(
                nameof(BroadcastingFacet.SendMyMessage),
                "foo",
                "Hello world!"
            ).AsCoroutine();
            yield return WaitForMessages(2);
            
            // check the callback was called
            Assert.False(client.onConnectionLostCalled);
            Assert.True(client.onConnectionRegainedCalled);
        }
        
        [UnityTest]
        public IEnumerator ItReceivesLostEventsOnReconnection()
        {
            /*
             * Send a message,
             * simulate a drop by decrementing lastEventId and
             * disabling/enabling the SSE socket.
             * Assert the last message was received again.
             */

            CreateClient();
            yield return WaitForClientToSettle();
            
            Assert.IsEmpty(client.receivedMessages);
            
            yield return OnFacet<BroadcastingFacet>.Call(
                nameof(BroadcastingFacet.SendMyMessage),
                "foo",
                "Hello world!"
            ).AsCoroutine();

            yield return WaitForMessages(1);
            
            var tunnel = ClientFacade.ClientApp.Services
                .Resolve<ClientBroadcastingManager>()
                .Tunnel;

            var idOfLastActuallyReceived = ((DefaultSseSocket)tunnel.Socket).state.LastReceivedEventId;
            
            Assert.AreEqual(1, client.receivedMessages.Count);
            Assert.IsInstanceOf<MyMessage>(client.receivedMessages[0]);
            Assert.AreEqual(
                "Hello world!",
                ((MyMessage) client.receivedMessages[0]).foo
            );
            
            // simulate drop
            
            tunnel.SocketGameObject.SetActive(false);
            
            yield return null;

            // repeat the last message only
            ((DefaultSseSocket)tunnel.Socket).state.LastReceivedEventId = idOfLastActuallyReceived - 1;
            tunnel.SocketGameObject.SetActive(true);
            
            // wait for the re-sending
            
            yield return WaitForMessages(2);
            
            Assert.AreEqual(2, client.receivedMessages.Count);
            Assert.IsInstanceOf<MyMessage>(client.receivedMessages[1]);
            Assert.AreEqual(
                "Hello world!",
                ((MyMessage) client.receivedMessages[1]).foo
            );
            
            yield return null;
        }
    }
}