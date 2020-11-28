using System;
using System.Collections;
using NUnit.Framework;
using Unisave.Facades;
using UnisaveFixture.Backend.Core.Broadcasting;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace UnisaveFixture.Tests.Core.Broadcasting
{
    public class BroadcastingTest
    {
        private const string ChannelParameterOne = "channel-parameter-one";
        private const string ChannelParameterTwo = "channel-parameter-two";
        
        private MyBroadcastingClient client;

        private void CreateClient(
            string channelParameter,
            bool sendMessageAfterSubscribing = false
        )
        {
            var go = new GameObject("MyBroadcastingClient");
            go.SetActive(false);
            client = go.AddComponent<MyBroadcastingClient>();
            client.channelParameter = channelParameter;
            client.sendMessageAfterSubscribing = sendMessageAfterSubscribing;
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
        public IEnumerator ItCanSubscribeToAChannelAndReceiveAMessage()
        {
            CreateClient(ChannelParameterOne);
            yield return WaitForClientToSettle();
            
            Assert.IsEmpty(client.receivedMessages);
            
            yield return OnFacet<BroadcastingFacet>.Call(
                nameof(BroadcastingFacet.SendMyMessage),
                ChannelParameterOne,
                "Hello world!"
            ).AsCoroutine();

            yield return WaitForMessages(1);
            
            Assert.AreEqual(1, client.receivedMessages.Count);
            Assert.IsInstanceOf<MyMessage>(client.receivedMessages[0]);
            Assert.AreEqual(
                "Hello world!",
                ((MyMessage) client.receivedMessages[0]).foo
            );
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator ItDoesntReceiveMessageForDifferentChannel()
        {
            CreateClient(ChannelParameterOne);
            yield return WaitForClientToSettle();
            
            Assert.IsEmpty(client.receivedMessages);
            
            yield return OnFacet<BroadcastingFacet>.Call(
                nameof(BroadcastingFacet.SendMyMessage),
                ChannelParameterTwo, // TWO
                "Send to another channel"
            ).AsCoroutine();
            
            // send one message afterwards for which we can wait
            yield return OnFacet<BroadcastingFacet>.Call(
                nameof(BroadcastingFacet.SendMyMessage),
                ChannelParameterOne,
                "Hello world!"
            ).AsCoroutine();

            // wait for the one message
            yield return WaitForMessages(1);
            
            // and check it's the hello-world one
            Assert.AreEqual(1, client.receivedMessages.Count);
            Assert.IsInstanceOf<MyMessage>(client.receivedMessages[0]);
            Assert.AreEqual(
                "Hello world!",
                ((MyMessage) client.receivedMessages[0]).foo
            );
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator ItRoutesMessagesByType()
        {
            CreateClient(ChannelParameterOne);
            yield return WaitForClientToSettle();
            
            Assert.IsEmpty(client.receivedMessages);
            
            yield return OnFacet<BroadcastingFacet>.Call(
                nameof(BroadcastingFacet.SendMyMessage),
                ChannelParameterOne,
                "Hello world!"
            ).AsCoroutine();
            
            yield return OnFacet<BroadcastingFacet>.Call(
                nameof(BroadcastingFacet.SendMyOtherMessage),
                ChannelParameterOne,
                42
            ).AsCoroutine();

            yield return WaitForMessages(2);
            
            Assert.AreEqual(2, client.receivedMessages.Count);
            Assert.AreEqual(2, client.calledMethods.Count);
            
            Assert.IsInstanceOf<MyMessage>(client.receivedMessages[0]);
            Assert.AreEqual(
                "Hello world!",
                ((MyMessage) client.receivedMessages[0]).foo
            );
            Assert.AreEqual(
                nameof(MyBroadcastingClient.OnMyMessage),
                client.calledMethods[0]
            );
            
            Assert.IsInstanceOf<MyOtherMessage>(client.receivedMessages[1]);
            Assert.AreEqual(
                42,
                ((MyOtherMessage) client.receivedMessages[1]).bar
            );
            Assert.AreEqual(
                nameof(MyBroadcastingClient.OnMyOtherMessage),
                client.calledMethods[1]
            );
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator ItDoesntReceiveMessagesAfterUnsubscribing()
        {
            CreateClient(ChannelParameterOne);
            yield return WaitForClientToSettle();
            
            Assert.IsEmpty(client.receivedMessages);
            
            yield return OnFacet<BroadcastingFacet>.Call(
                nameof(BroadcastingFacet.SendMyMessage),
                ChannelParameterOne,
                "Hello world!"
            ).AsCoroutine();
            
            yield return WaitForMessages(1);
            
            // disable --> unsubscribes
            client.gameObject.SetActive(false);
            
            // clear log
            client.receivedMessages.Clear();
            
            // send a message
            yield return OnFacet<BroadcastingFacet>.Call(
                nameof(BroadcastingFacet.SendMyMessage),
                ChannelParameterOne,
                "Hello world!"
            ).AsCoroutine();
            
            // wait a sec.
            yield return new WaitForSeconds(1f);
            
            // nothing
            Assert.IsEmpty(client.receivedMessages);

            yield return null;
        }

        [UnityTest]
        public IEnumerator PendingSubscriptionsWork()
        {
            /*
             * Sending a message into the channel just after subscription
             * should be received by the client even though the client starts
             * handling the subscription only after the message is already sent.
             */
            
            // SETUP: establish the SSE connection so that the server
            // will send us the message even though we don't handle the
            // subscription yet. Without an existing connection the message
            // would be buffered on the server.
            
            CreateClient("completely-different-channel", false);
            yield return WaitForClientToSettle();
            client = null;

            // now the proper test:
            
            CreateClient(ChannelParameterOne, true);
            yield return WaitForClientToSettle();

            yield return WaitForMessages(1);
            
            Assert.AreEqual(1, client.receivedMessages.Count);
            Assert.IsInstanceOf<MyMessage>(client.receivedMessages[0]);
            Assert.AreEqual(
                "Message after subscribing",
                ((MyMessage) client.receivedMessages[0]).foo
            );
            
            yield return null;
        }
    }
}