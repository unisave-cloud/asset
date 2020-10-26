namespace Unisave.Broadcasting
{
    // TODO: move to framework, to facades
    public static class Broadcast
    {
        public static ChannelContext Channel<TChannel>(
            params string[] parameters
        ) where TChannel : BroadcastingChannel
        {
            return new ChannelContext();
        }
    }

    public class ChannelContext
    {
        public void Send(/* BroadcastingMessage msg */)
        {
            
        }
    }
}