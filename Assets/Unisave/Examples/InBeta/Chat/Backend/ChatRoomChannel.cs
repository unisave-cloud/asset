using Unisave.Broadcasting;
using Unisave.Facades;

namespace Unisave.Examples.ChatDoodle.Backend
{
    public class ChatRoomChannel : BroadcastingChannel
    {
        public SpecificChannel WithParameters(string roomName)
        {
            return SpecificChannel.From<ChatRoomChannel>(roomName);
        }
    }
}