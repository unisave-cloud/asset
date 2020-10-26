using Unisave.Broadcasting;
using Unisave.Facets;

namespace Unisave.Examples.ChatDoodle.Backend
{
    public class ChattingFacet : Facet
    {
        public void SendMessage(string nickname, string message)
        {
            Broadcast.Channel<ChattingRoomChannel>("<room-id>")
                .Send(/* BroadcastingMessage instance */);
        }
    }
}