using System;
using System.Collections;
using System.Text;
using LightJson;
using UnityEngine;
using UnityEngine.Networking;

namespace Unisave.Examples.ChatDoodle
{
    // TODO: this will be part of the Unisave asset
    
    // https://javascript.info/server-sent-events
    
    public class BroadcastingClient : MonoBehaviour
    {
        private UnityWebRequest runningRequest;

        private void OnEnable()
        {
            if (runningRequest == null)
            {
                Debug.Log("Starting the coroutine...");
                StartCoroutine(ListeningLoop());
            }
        }

        private void OnDisable()
        {
            if (runningRequest != null)
            {
                Debug.Log("Killing the request...");
                runningRequest.Abort();
                runningRequest = null;
            }
        }
        
        private IEnumerator ListeningLoop()
        {
            if (runningRequest != null)
                throw new InvalidOperationException(
                    "A request is already running."
                );
            
            runningRequest = new UnityWebRequest(
                "http://localhost:3000/broadcast-time", // TODO: localhost/_broadcasting/...
                "GET",
                new DownloadHandler(HandleMessage),
                null
            );
            
            runningRequest.SetRequestHeader("Accept", "text/event-stream");
            
            Debug.Log("STARTING");
            
            yield return runningRequest.SendWebRequest();
            
            Debug.Log("DONE");
            Debug.Log(runningRequest.error);
            
            runningRequest.Dispose();
        }

        private void HandleMessage(JsonValue message)
        {
            Debug.Log("Message received: " + message.ToString());
        }

        private class DownloadHandler : DownloadHandlerScript
        {
            private StringBuilder textStream = new StringBuilder();
            private Action<JsonValue> messageHandler;

            public DownloadHandler(Action<JsonValue> messageHandler)
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
                messageHandler.Invoke(new JsonValue(message));
            }
        }
    }
}