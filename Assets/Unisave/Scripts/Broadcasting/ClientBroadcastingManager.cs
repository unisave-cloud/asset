using System;
using Unisave.Foundation;
using Unisave.Http;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// The client application service that handles broadcasting
    /// </summary>
    public class ClientBroadcastingManager : IDisposable
    {
        public BroadcastingTunnel Tunnel { get; }
        public SubscriptionRouter SubscriptionRouter { get; }

        public ClientBroadcastingManager(ClientApplication app)
        {
            Tunnel = new BroadcastingTunnel(app);
            SubscriptionRouter = new SubscriptionRouter(
                Tunnel,
                app.Resolve<AssetHttpClient>()
            );
        }

        public void Dispose()
        {
            SubscriptionRouter?.Dispose();
            Tunnel?.Dispose();
        }
    }
}