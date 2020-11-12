namespace Unisave.Broadcasting.Sse
{
    /// <summary>
    /// Represents a single message received via the SSE protocol
    /// https://javascript.info/server-sent-events
    /// </summary>
    public struct SseMessage
    {
        /// <summary>
        /// Optional event name
        /// </summary>
        public string @event;
        
        /// <summary>
        /// Message data
        /// </summary>
        public string data;
        
        /// <summary>
        /// Message id
        /// </summary>
        public int? id;
        
        /// <summary>
        /// Recommended retry delay in milliseconds
        /// (retry, when the connection unexpectedly breaks)
        /// </summary>
        public int? retry;

        public static SseMessage Parse(string text)
        {
            // TODO: implement this
            
            return new SseMessage {
                data = text,
                @event = null,
                id = null,
                retry = null
            };
        }
    }
}