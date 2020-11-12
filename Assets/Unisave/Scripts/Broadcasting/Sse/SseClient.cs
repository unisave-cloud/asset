using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Unisave.Broadcasting.Sse
{
    // https://javascript.info/server-sent-events
    
    public class SseClient : MonoBehaviour
    {
        /// <summary>
        /// Called when a new message arrives
        /// </summary>
        public event Action<SseMessage> OnMessageReceived;
        
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
                new SseDownloadHandler(HandleMessage),
                null
            );
            
            runningRequest.SetRequestHeader("Accept", "text/event-stream");
            
            Debug.Log("STARTING");
            
            yield return runningRequest.SendWebRequest();
            
            Debug.Log("DONE");
            Debug.Log(runningRequest.error);
            
            runningRequest.Dispose();
        }

        private void HandleMessage(SseMessage message)
        {
            Debug.Log("SSE message received: " + message.data);
            
            OnMessageReceived?.Invoke(message);
        }
    }
}