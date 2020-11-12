using System;
using System.Threading.Tasks;
using LightJson;
using Unisave.Broadcasting;
using Unisave.Examples.ChatDoodle.Backend;
using UnityEngine;

namespace Unisave.Examples.ChatDoodle
{
    // TODO: use this message
    public class PlayerJoinedMessage {}
    
    public class ChatController : UnisaveBroadcastingClient
    {
        async void Start()
        {
            (await SubscribeTo<ChattingRoomChannel>("<chatting-room-id>"))
                .Forward<ChatMessage>(ChatMessageReceived)
                .Forward<PlayerJoinedMessage>(PlayerJoined);
        }

        void ChatMessageReceived(ChatMessage msg)
        {
            Debug.Log($"[{msg.nickname}]: {msg.message}");
        }

        void PlayerJoined(PlayerJoinedMessage msg)
        { /* ... */ }
    }
}