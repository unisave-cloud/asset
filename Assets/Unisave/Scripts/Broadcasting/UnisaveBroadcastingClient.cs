using System.Threading.Tasks;
using LightJson;
using UnityEngine;

namespace Unisave.Broadcasting
{
    public abstract class UnisaveBroadcastingClient : MonoBehaviour
    {
        protected Task SubscribeToChannelAsync(string channelId)
        {
            // throw authentication exception or succeed (or connection exception)
            
            return Task.CompletedTask;
        }
        
        protected virtual void OnDropletReceived(string channelId, JsonObject droplet)
        {
            // TODO: bake channel id into the droplet itself ??
        }
        
        protected virtual void OnDisable()
        {
            // stop listening to all channels
        }
    }
}