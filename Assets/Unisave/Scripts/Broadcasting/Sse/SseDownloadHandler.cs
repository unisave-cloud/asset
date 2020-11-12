using System;
using System.Text;
using UnityEngine.Networking;

namespace Unisave.Broadcasting.Sse
{
    public class SseDownloadHandler : DownloadHandlerScript
    {
        private readonly StringBuilder textStream = new StringBuilder();
        private readonly Action<SseMessage> messageHandler;

        public SseDownloadHandler(Action<SseMessage> messageHandler)
        {
            this.messageHandler = messageHandler
                ?? throw new ArgumentNullException(nameof(messageHandler));
        }
        
        protected override bool ReceiveData(byte[] receivedData, int dataLength)
        {
            textStream.Append(
                Encoding.UTF8.GetString(receivedData, 0, dataLength)
            );
            
            ExtractMessages();

            // continue receiving data
            return true;
        }

        private void ExtractMessages()
        {
            while (true)
            {
                int length = GetMessageLength();
                
                if (length == 0)
                    break;

                char[] message = new char[length];
                textStream.CopyTo(0, message, 0, length);
                textStream.Remove(0, length);
                
                HandleMessage(new string(message));
            }
        }

        private int GetMessageLength()
        {
            bool wasNewline = false;

            for (int i = 0; i < textStream.Length; i++)
            {
                if (textStream[i] != '\n')
                {
                    wasNewline = false;
                    continue;
                }

                if (wasNewline)
                {
                    return i + 1;
                }

                wasNewline = true;
            }

            return 0;
        }

        private void HandleMessage(string message)
        {
            var parsed = SseMessage.Parse(message);

            messageHandler.Invoke(parsed);
        }
    }
}