using System;
using System.Collections;
using System.Text;
using LightJson;
using Unisave.Foundation;
using Unisave.Sessions;
using Unisave.Utils;
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

        public BroadcastingConnection connectionState
            = BroadcastingConnection.Disconnected;
        
        private UnityWebRequest runningRequest;

        private ClientApplication app;
        
        private int lastReceivedMessageId = 0;
        private int retryMilliseconds = 15_000;
        private bool isRetrying = false;

        /// <summary>
        /// Call this right after this component is created
        /// </summary>
        public void Initialize(
            ClientApplication app,
            int lastReceivedMessageId
        )
        {
            this.app = app;
            this.lastReceivedMessageId = lastReceivedMessageId;
        }

        /// <summary>
        /// Enabling the component acts as a wanted connection / reconnection
        /// </summary>
        private void OnEnable()
        {
            if (runningRequest == null)
            {
                StartCoroutine(ListeningLoop());
            }
        }

        /// <summary>
        /// Disabling the component acts as a wanted disconnection
        /// </summary>
        private void OnDisable()
        {
            if (runningRequest != null)
            {
                runningRequest.Abort();
                runningRequest = null;
                isRetrying = false;
                connectionState = BroadcastingConnection.Disconnected;
            }
        }
        
        private IEnumerator ListeningLoop()
        {
            if (runningRequest != null)
                throw new InvalidOperationException(
                    "A request is already running."
                );
            
            // skip a frame so that the Initialize
            // method gets called
            yield return null;
            
            // handle retrying
            if (isRetrying)
            {
                isRetrying = false;

                yield return new WaitForSeconds(retryMilliseconds / 1000f);
            }
            
            // === THE LOOP ===

            var downloadHandler = new SseDownloadHandler(HandleMessage);
            downloadHandler.OnDataReceived += AppendToDebugLog;
            
            var url = app.Resolve<ApiUrl>();
            var sessionIdRepo = app.Resolve<SessionIdRepository>();
            
            runningRequest = new UnityWebRequest(
                url.BroadcastingListen(),
                "POST",
                downloadHandler,
                new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(
                        new JsonObject {
                            ["gameToken"] = app.Preferences.GameToken,
                            ["editorKey"] = app.Preferences.EditorKey,
                            ["buildGuid"] = Application.buildGUID,
                            ["backendHash"] = app.Preferences.BackendHash,
                            ["sessionId"] = sessionIdRepo.GetSessionId(),
                            ["lastReceivedMessageId"] = lastReceivedMessageId
                        }.ToString()
                    )
                )
            );
            
            runningRequest.SetRequestHeader("Content-Type", "application/json");
            runningRequest.SetRequestHeader("Accept", "text/event-stream");
            
            // === LISTEN ===
            
            connectionState = BroadcastingConnection.Connected;
            
            yield return runningRequest.SendWebRequest();
            
            connectionState = BroadcastingConnection.Reconnecting;
            
            // === HANDLE BREAKAGE ===

            Debug.LogWarning(
                $"[Unisave] Broadcasting client connection broke, " +
                $"retrying in {retryMilliseconds}ms"
            );
            
            isRetrying = true;
            runningRequest.Dispose();
            runningRequest = null;
            
            StartCoroutine(ListeningLoop());
        }

        private void HandleMessage(SseMessage message)
        {
            if (message.id != null)
                lastReceivedMessageId = (int) message.id;

            if (message.retry != null)
                retryMilliseconds = (int) message.retry;
            
            OnMessageReceived?.Invoke(message);
        }
    }
}