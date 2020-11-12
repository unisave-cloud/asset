using System;

namespace Unisave.Broadcasting
{
    // TODO: maybe obsolete? Or a container for the sub-classes?
    
    /// <summary>
    /// The client application service that handles broadcasting
    /// </summary>
    public class BroadcastingReceiver
    {
        public void RegisterChannelListener(
            string channelName,
            Action<string, BroadcastingMessage> listener
        )
        {
            
        }

        public void ReleaseChannelListener(
            string channelName,
            Action<string, BroadcastingMessage> listener
        )
        {
            
        }
    }
}