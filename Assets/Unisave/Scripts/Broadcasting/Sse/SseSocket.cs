using System;
using System.Collections;
using System.Text;
using LightJson;
using Unisave.Foundation;
using UnityEngine;
using UnityEngine.Networking;
using Application = UnityEngine.Application;

namespace Unisave.Broadcasting.Sse
{
    // https://javascript.info/server-sent-events
    
    /// <summary>
    /// Represents an SSE connection to the Unisave broadcasting server
    /// </summary>
    public partial class SseSocket : MonoBehaviour
    {
        /// <summary>
        /// Called when a new message arrives
        /// </summary>
        public event Action<SseMessage> OnMessageReceived;
        
        private UnityWebRequest runningRequest;

        private ClientApplication app;

        /// <summary>
        /// Call this right after this component is created
        /// </summary>
        /// <param name="givenApp"></param>
        public void SetClientApplication(ClientApplication givenApp)
        {
            app = givenApp;
        }

        private void OnEnable()
        {
            if (runningRequest == null)
            {
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
            // skip a frame so that the SetClientApplication
            // method gets called
            yield return null;
            
            if (runningRequest != null)
                throw new InvalidOperationException(
                    "A request is already running."
                );

            var downloadHandler = new SseDownloadHandler(HandleMessage);
            downloadHandler.OnDataReceived += AppendToDebugLog;
            
            runningRequest = new UnityWebRequest(
                "https://localhost/_broadcasting/listen",
                "POST",
                downloadHandler,
                new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(
                        new JsonObject {
                            ["gameToken"] = app.Preferences.GameToken,
                            ["editorKey"] = app.Preferences.EditorKey,
                            ["buildGuid"] = Application.buildGUID,
                            ["backendHash"] = app.Preferences.BackendHash,
                            ["sessionId"] = "<session-id>" // TODO: fix this
                            // TODO: send lastReceivedMessageId
                        }.ToString()
                    )
                )
            );
            
            runningRequest.SetRequestHeader("Content-Type", "application/json");
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