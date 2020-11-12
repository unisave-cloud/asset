using Unisave.Broadcasting;
using Unisave.Facades;
using Unisave.Facets;

namespace Unisave.Examples.ChatDoodle.Backend
{
    public class ChatFacet : Facet
    {
        public void SendMessage(string nickname, string message)
        {
            Broadcast.Channel<ChatRoomChannel>()
                .WithParameters("<room-id>")
                .Send(new ChatMessage {
                    nickname = nickname,
                    message = message
                });
        }

        public ChannelSubscription JoinRoom()
        {
            // verify the player can access the channel
            // ...

            // subscribe the client into the channel
            var subscription = Broadcast.Channel<ChatRoomChannel>()
                .WithParameters("<room-id>")
                .CreateSubscription();
            
            // new subscriber broadcast
            Broadcast.Channel<ChatRoomChannel>()
                .WithParameters("<room-id>")
                .Send(new PlayerJoinedMessage());

            return subscription;
        }
    }
}