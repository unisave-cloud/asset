using System;
using System.Collections;
using System.Collections.Generic;
using Unisave.Facades;
using UnityEngine;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// Parent class for any MonoBehaviour that wants to subscribe to channels
    /// </summary>
    public abstract class UnisaveBroadcastingClient : MonoBehaviour
    {
        /// <summary>
        /// Subscription this client owns
        /// </summary>
        private readonly HashSet<ChannelSubscription> subscriptions
            = new HashSet<ChannelSubscription>();
        
        /// <summary>
        /// Receive messages from a channel subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        protected MessageRouterBuilder FromSubscription(
            ChannelSubscription subscription
        )
        {
            if (subscriptions.Contains(subscription))
                throw new InvalidOperationException(
                    "You cannot handle a subscription twice"
                );

            subscriptions.Add(subscription);

            var messageRouter = new MessageRouter();
            
            StartCoroutine(
                RegisterSubscriptionHandlerAfterDelay(
                    subscription,
                    messageRouter
                )
            );

            return new MessageRouterBuilder(messageRouter);
        }

        private IEnumerator RegisterSubscriptionHandlerAfterDelay(
            ChannelSubscription subscription,
            MessageRouter messageRouter
        )
        {
            // skip a frame to make sure the router is fully built
            // before handling any messages
            yield return null;
            
            // make sure the subscription is still active
            // (we haven't been killed in the mean time)
            if (!subscriptions.Contains(subscription))
                yield break;
            
            // register the handler
            var subscriptionRouter = GetSubscriptionRouter();
            subscriptionRouter.HandleSubscription(
                subscription,
                messageRouter.RouteMessage
            );
        }

        protected virtual void OnDisable()
        {
            var subscriptionRouter = GetSubscriptionRouter();
            
            foreach (var sub in subscriptions)
                subscriptionRouter.EndSubscription(sub);

            subscriptions.Clear();
        }

        private SubscriptionRouter GetSubscriptionRouter()
        {
            var manager = ClientFacade.ClientApp
                .Resolve<ClientBroadcastingManager>();

            return manager.SubscriptionRouter;
        }
    }
}