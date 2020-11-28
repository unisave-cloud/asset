using System.Collections.Generic;
using Unisave.Broadcasting;
using Unisave.Facades;
using UnisaveFixture.Backend.Core.Broadcasting;
using UnityEngine;

namespace UnisaveFixture.Tests.Core.Broadcasting
{
    public class MyBroadcastingClient : UnisaveBroadcastingClient
    {
        // to inspect the received messages
        public List<BroadcastingMessage> receivedMessages
            = new List<BroadcastingMessage>();
        
        // to check proper method calling
        public List<string> calledMethods = new List<string>();

        // subscription parameters
        public string channelParameter;
        public bool sendMessageAfterSubscribing;

        // set to true when the OnEnable method finishes
        public bool hasSettled = false;
        
        private async void OnEnable()
        {
            var subscription = await OnFacet<BroadcastingFacet>
                .CallAsync<ChannelSubscription>(
                    nameof(BroadcastingFacet.SubscribeToMyChannel),
                    channelParameter,
                    sendMessageAfterSubscribing
                );
            
            FromSubscription(subscription)
                .Forward<MyMessage>(OnMyMessage)
                .Forward<MyOtherMessage>(OnMyOtherMessage)
                .ElseLogWarning();

            // now we can start doing experiments
            hasSettled = true;
        }

        public void OnMyMessage(MyMessage msg)
        {
            receivedMessages.Add(msg);
            calledMethods.Add(nameof(OnMyMessage));
        }

        public void OnMyOtherMessage(MyOtherMessage msg)
        {
            receivedMessages.Add(msg);
            calledMethods.Add(nameof(OnMyOtherMessage));
        }
    }
}