using System;
using System.Collections.Generic;
using LightJson;
using Unisave.Serialization;
using Unisave.Serialization.Context;
using UnityEngine;
using Subscription = Unisave.Broadcasting.ChannelSubscription;
using Message = Unisave.Broadcasting.BroadcastingMessage;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// Routes messages from the tunnel to individual subscriptions
    /// </summary>
    public class SubscriptionRouter : IDisposable
    {
        private readonly BroadcastingTunnel tunnel;

        private Dictionary<Subscription, Action<Message>> activeSubscriptions
            = new Dictionary<Subscription, Action<Message>>();
        
        private Dictionary<Subscription, Queue<Message>> pendingSubscriptions
            = new Dictionary<Subscription, Queue<Message>>();
        
        public SubscriptionRouter(BroadcastingTunnel tunnel)
        {
            this.tunnel = tunnel;
            
            tunnel.OnEventReceived += OnTunnelEventReceived;
        }
        
        private void OnTunnelEventReceived(JsonObject e)
        {
            switch (e["type"].AsString)
            {
                case "subscription":
                    var sub = Serializer.FromJson<ChannelSubscription>(
                        e["subscription"],
                        DeserializationContext.BroadcastingContext()
                    );
                    CreatePendingSubscription(sub);
                    break;
                
                case "message":
                    var message = Serializer.FromJson<BroadcastingMessage>(
                        e["message"],
                        DeserializationContext.BroadcastingContext()
                    );
                    RouteMessage(e["channel"].AsString, message);
                    break;
                
                default:
                    Debug.LogWarning(
                        "Broadcasting tunnel received a message " +
                        "of unknown type:\n" + e.ToString(true)
                    );
                    break;
            }
        }

        private void CreatePendingSubscription(ChannelSubscription subscription)
        {
            // TODO: handle timeout 5min and delete the pending sub. and print warning
            
            throw new NotImplementedException();
        }

        private void RouteMessage(string channel, BroadcastingMessage message)
        {
            foreach (var pair in activeSubscriptions)
            {
                if (pair.Key.ChannelName == channel)
                {
                    InvokeHandlerSafely(pair.Value, message);
                }
            }
            
            foreach (var pair in pendingSubscriptions)
            {
                if (pair.Key.ChannelName == channel)
                {
                    pair.Value.Enqueue(message);
                    CheckPendingSubscriptionExpiration(pair.Key);
                }
            }
        }

        private void CheckPendingSubscriptionExpiration(
            ChannelSubscription subscription
        )
        {
            // TODO: make pending subscription into a class
            
            throw new NotImplementedException();
        }
        
        public void HandleSubscription(
            ChannelSubscription subscription,
            Action<BroadcastingMessage> handler
        )
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));
            
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            
            if (activeSubscriptions.ContainsKey(subscription))
                throw new InvalidOperationException(
                    "This subscription is already being handled"
                );
            
            activeSubscriptions[subscription] = handler;
            
            if (pendingSubscriptions.ContainsKey(subscription))
            {
                var messages = pendingSubscriptions[subscription];
                pendingSubscriptions.Remove(subscription);
                
                while (messages.Count > 0)
                    InvokeHandlerSafely(handler, messages.Dequeue());
            }
        }

        private void InvokeHandlerSafely(
            Action<BroadcastingMessage> handler,
            BroadcastingMessage message
        )
        {
            try
            {
                handler?.Invoke(message);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        public void EndSubscriptions(IEnumerable<ChannelSubscription> subscriptions)
        {
            // TODO: sends a request to the server that this subscription(s)
            // is dead and it should no longer send messages from the
            // corresponding channel, if this was the last subscription to it
            
            // if the subscription isn't known, it's ok, do nothing
            
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            tunnel.OnEventReceived -= OnTunnelEventReceived;
            
            // TODO: end all subscriptions
        }
    }
}