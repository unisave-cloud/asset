using System;
using System.Text;

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
            string[] lines = text.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries
            );
            
            var message = new SseMessage {
                data = null,
                @event = null,
                id = null,
                retry = null
            };
            
            StringBuilder data = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.StartsWith("id: "))
                    message.id = int.Parse(line.Substring(4));
                else if (line.StartsWith("event: "))
                    message.@event = line.Substring(7);
                else if (line.StartsWith("retry: "))
                    message.retry = int.Parse(line.Substring(7));
                else if (line.StartsWith("data: "))
                    data.Append(line.Substring(6));
            }

            message.data = data.ToString();
            
            return message;
        }
    }
}