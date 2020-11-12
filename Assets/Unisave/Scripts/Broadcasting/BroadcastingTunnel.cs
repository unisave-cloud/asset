using System;
using LightJson;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// The tunnel that transports messages from the server to the client
    /// (all the channels combined with all the metadata)
    /// </summary>
    public class BroadcastingTunnel
    {
        /// <summary>
        /// Called when a new event arrives through the tunnel
        /// </summary>
        public event Action<JsonObject> OnEventReceived;
    }
}