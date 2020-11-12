using Unisave.Broadcasting;
using Unisave.Examples.ChatDoodle.Backend;
using Unisave.Facades;
using UnityEngine;

namespace Unisave.Examples.ChatDoodle
{
    public class ChatController : UnisaveBroadcastingClient
    {
        private async void OnEnable()
        {
            var subscription = await OnFacet<ChatFacet>
                .CallAsync<ChannelSubscription>(nameof(ChatFacet.JoinRoom));
            
            FromSubscription(subscription)
                .Forward<ChatMessage>(ChatMessageReceived)
                .Forward<PlayerJoinedMessage>(PlayerJoined)
                .ElseLogWarning();
        }

        void ChatMessageReceived(ChatMessage msg)
        {
            Debug.Log($"[{msg.nickname}]: {msg.message}");
        }

        void PlayerJoined(PlayerJoinedMessage msg)
        {
            Debug.Log("Someone joined the room.");
        }
    }
}