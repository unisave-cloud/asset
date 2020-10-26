using System;
using System.Threading.Tasks;
using LightJson;
using Unisave.Broadcasting;
using Unisave.Examples.ChatDoodle.Backend;
using UnityEngine;

namespace Unisave.Examples.ChatDoodle
{
    public class ChatMessage {}
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
        { /* ... */ }

        void PlayerJoined(PlayerJoinedMessage msg)
        { /* ... */ }
    }
}