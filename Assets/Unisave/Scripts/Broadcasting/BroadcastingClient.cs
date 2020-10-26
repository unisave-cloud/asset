using System;
using System.Threading.Tasks;
using LightJson;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// Handles subscribing to channels and receiving droplets
    /// </summary>
    public class BroadcastingClient
    {
        public async Task SubscribeAsync(string channelId, Action<JsonObject> listener)
        {
            throw new NotImplementedException();
        }

        public async Task UnsubscribeAsync(Action<JsonObject> listener)
        {
            throw new NotImplementedException();
        }
    }
}