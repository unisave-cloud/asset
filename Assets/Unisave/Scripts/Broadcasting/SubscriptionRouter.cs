using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Foundation;
using Unisave.Http;
using Unisave.Serialization;
using Unisave.Serialization.Context;
using Unisave.Sessions;
using Unisave.Utils;
using UnityEngine;
using Application = UnityEngine.Application;
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
        private readonly ClientApplication app;

        private readonly AssetHttpClient http;
        private readonly ApiUrl url;
        private readonly ClientSessionIdRepository sessionIdRepository;

        /// <summary>
        /// Active subscriptions that are both set up on the server
        /// and consumed here by the client
        /// </summary>
        private Dictionary<Subscription, Action<Message>> activeSubscriptions
            = new Dictionary<Subscription, Action<Message>>();
        
        /// <summary>
        /// Pending subscriptions are subscriptions that have been set up
        /// on the server, but are not yet consumed here, by the client
        /// </summary>
        private Dictionary<Subscription, Queue<Message>> pendingSubscriptions
            = new Dictionary<Subscription, Queue<Message>>();
        
        public SubscriptionRouter(
            BroadcastingTunnel tunnel,
            ClientApplication app
        )
        {
            this.tunnel = tunnel;
            this.app = app;
            
            http = app.Resolve<AssetHttpClient>();
            url = app.Resolve<ApiUrl>();
            sessionIdRepository = app.Resolve<ClientSessionIdRepository>();

            tunnel.OnMessageEvent += OnMessageEvent;
            tunnel.OnSubscriptionEvent += OnSubscriptionEvent;
        }

        private void OnMessageEvent(JsonObject data)
        {
            var message = Serializer.FromJson<BroadcastingMessage>(
                data["message"],
                DeserializationContext.BroadcastingContext()
            );
            
            RouteMessage(data["channel"].AsString, message);
        }

        private void OnSubscriptionEvent(JsonObject data)
        {
            // TODO:
//            var sub = Serializer.FromJson<ChannelSubscription>(
//                data["subscription"],
//                DeserializationContext.BroadcastingContext()
//            );
//            CreatePendingSubscription(sub);
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
            
            // to remove
            EndSubscriptions(new[] { subscription });

            // if expires
            CheckTunnelNeededness();
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
            
            tunnel.IsNeeded();
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
            JsonArray channelsToUnsubscribe = new JsonArray();
            
            foreach (var sub in subscriptions)
            {
                bool lastForChannel = EndSubscription(sub);
                
                if (lastForChannel)
                    channelsToUnsubscribe.Add(sub.ChannelName);
            }

            if (channelsToUnsubscribe.Count > 0)
            {
                // NOTE: It's important that the callback will remain null,
                // since this request could be fired during disposal
                // and we cannot wait for the response then.
                // All sorts of unity errors would show up in the console.
            
                http.Post(
                    url.BroadcastingUnsubscribe(),
                    new JsonObject {
                        ["gameToken"] = app.Preferences.GameToken,
                        ["editorKey"] = app.Preferences.EditorKey,
                        ["buildGuid"] = Application.buildGUID,
                        ["backendHash"] = app.Preferences.BackendHash,
                        ["sessionId"] = sessionIdRepository.GetSessionId(),
                        ["channels"] = channelsToUnsubscribe
                    },
                    null // IMPORTANT! see the note above
                );
            }

            CheckTunnelNeededness();
        }

        /// <summary>
        /// Ends a subscription and returns true if it was
        /// the last subscriptions for this channel, thus we should
        /// unsubscribe from the channel
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        private bool EndSubscription(ChannelSubscription subscription)
        {
            activeSubscriptions.Remove(subscription);
            pendingSubscriptions.Remove(subscription);

            if (activeSubscriptions.Keys
                .Any(s => s.ChannelName == subscription.ChannelName))
                return false;
            
            if (pendingSubscriptions.Keys
                .Any(s => s.ChannelName == subscription.ChannelName))
                return false;

            return true;
        }

        /// <summary>
        /// Checks whether the tunnel is needed and if not, it will call
        /// the tunnel.IsNotNeeded() method on it.
        /// </summary>
        private void CheckTunnelNeededness()
        {
            if (activeSubscriptions.Count > 0)
                return;
            
            if (pendingSubscriptions.Count > 0)
                return;
            
            tunnel.IsNotNeeded();
        }

        public void Dispose()
        {
            List<ChannelSubscription> subscriptions = new List<ChannelSubscription>();
            subscriptions.AddRange(activeSubscriptions.Keys);
            subscriptions.AddRange(pendingSubscriptions.Keys);
            EndSubscriptions(subscriptions);
            
            tunnel.IsNotNeeded();
            
            tunnel.OnMessageEvent -= OnMessageEvent;
            tunnel.OnSubscriptionEvent -= OnSubscriptionEvent;
        }
    }
}