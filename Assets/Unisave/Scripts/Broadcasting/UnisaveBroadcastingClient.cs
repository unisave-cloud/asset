using System;
using System.Threading.Tasks;
using LightJson;
using UnityEngine;

namespace Unisave.Broadcasting
{
    public class SubscriptionRouter
    {
        public SubscriptionRouter Forward<TMessage>(Action<TMessage> action)
        {
            return this;
        }
    }
    
    public abstract class UnisaveBroadcastingClient : MonoBehaviour
    {
        protected async Task<SubscriptionRouter> SubscribeTo<TChannel>(
            params string[] parameters
        ) where TChannel : BroadcastingChannel
        {
            // throw authentication exception or succeed (or connection exception)

            await Task.Yield();
            
            return new SubscriptionRouter();
        }
        
        protected virtual void OnDisable()
        {
            // stop listening to all channels
        }
    }
}