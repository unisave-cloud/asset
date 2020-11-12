using System;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// The client application service that handles broadcasting
    /// </summary>
    public class ClientBroadcastingManager : IDisposable
    {
        public BroadcastingTunnel Tunnel { get; }
        public SubscriptionRouter SubscriptionRouter { get; }
        
        public ClientBroadcastingManager()
        {
            Tunnel = new BroadcastingTunnel();
            SubscriptionRouter = new SubscriptionRouter(Tunnel);
        }

        public void Dispose()
        {
            SubscriptionRouter?.Dispose();
            Tunnel?.Dispose();
        }
    }
}