using System;
using LightJson;
using Unisave.Broadcasting.Sse;
using Unisave.Foundation;
using Unisave.Serialization;
using UnityEngine;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// The tunnel that transports messages from the server to the client
    /// (all the channels combined with all the metadata)
    /// </summary>
    public class BroadcastingTunnel : IDisposable
    {
        /// <summary>
        /// Called when a new event arrives through the tunnel
        /// </summary>
        public event Action<JsonObject> OnEventReceived;

        private SseClient sseClient;

        public BroadcastingTunnel(ClientApplication app)
        {
            // create the game object
            GameObject go = new GameObject("UnisaveSseClient");
            sseClient = go.AddComponent<SseClient>();
            
            sseClient.SetClientApplication(app);
            sseClient.OnMessageReceived += OnSseMessageReceived;
        }

        private void OnSseMessageReceived(SseMessage message)
        {
            // TODO: dummy implementation
            
            var m = new Unisave.Examples.ChatDoodle.Backend.ChatMessage {
                message = message.data,
                nickname = "NOPE"
            };
            
            OnEventReceived?.Invoke(new JsonObject {
                ["type"] = "message",
                ["message"] = Serializer.ToJson<BroadcastingMessage>(m)
            });
        }

        public void Dispose()
        {
            if (sseClient != null)
            {
                sseClient.OnMessageReceived -= OnSseMessageReceived;
                sseClient = null;
            }
        }
    }
}