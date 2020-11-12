using Unisave.Broadcasting;

namespace Unisave.Examples.ChatDoodle.Backend
{
    public class ChatMessage : BroadcastingMessage
    {
        public string nickname;
        public string message;
    }
}