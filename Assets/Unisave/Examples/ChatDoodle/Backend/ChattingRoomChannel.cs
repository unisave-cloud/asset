using Unisave.Broadcasting;
using Unisave.Facades;

namespace Unisave.Examples.ChatDoodle.Backend
{
    public class ChattingRoomChannel : BroadcastingChannel
    {
        public override int ParameterCount => 1;

        public override bool Join(params string[] parameters)
        {
            // broadcast player joined message
            Broadcast.Channel<ChattingRoomChannel>("<room-id>")
                .Send(new ChatMessage {
                    nickname = "Server",
                    message = "Someone joined the room"
                });
            
            return true;
        }
    }
}