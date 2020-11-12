using System;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// Routes messages from the tunnel to individual subscriptions
    /// </summary>
    public class SubscriptionRouter
    {
        public void HandleSubscription(
            ChannelSubscription subscription,
            Action<BroadcastingMessage> handler
        )
        {
            // TODO: move the subscription from pending to active or
            // create a new active subscription if none are pending
            
            // TODO: throw if subscription already used
            
            // TODO: flush pending messages (while the sub. was pending)
            
            throw new NotImplementedException();
        }
        
        public void EndSubscription(ChannelSubscription subscription)
        {
            // TODO: sends a request to the server that this subscription
            // is dead and it should no longer send messages from the
            // corresponding channel, if this was the last subscription to it
            
            // if the subscription isn't known, it's ok, do nothing
            
            throw new NotImplementedException();
        }
    }
}