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

        private UnityWebRequest runningRequest;

        private ClientApplication app;
        
        public int lastReceivedMessageId = 0;
        public int retryMilliseconds = 15_000;
        public bool isRetrying = false;
        public BroadcastingConnection connectionState
            = BroadcastingConnection.Disconnected;

        private bool intendedDisconnection = false;

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
                intendedDisconnection = true;
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
                
                #if UNITY_EDITOR
                AppendToDebugLog("WILL RETRY SOON\n\n");
                #endif

                yield return new WaitForSeconds(retryMilliseconds / 1000f);
            }
            
            // === THE LOOP ===

            var downloadHandler = new SseDownloadHandler(HandleMessage);
            
            #if UNITY_EDITOR
            downloadHandler.OnDataReceived += AppendToDebugLog;
            #endif
            
            var url = app.Resolve<ApiUrl>();
            var sessionIdRepo = app.Resolve<ClientSessionIdRepository>();
            
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
            intendedDisconnection = false;
            
            #if UNITY_EDITOR
            AppendToDebugLog("CONNECTING\n\n");
            #endif
            
            yield return runningRequest.SendWebRequest();

            #if UNITY_EDITOR
            AppendToDebugLog("DISCONNECTED\n\n");
            #endif
            
            connectionState = BroadcastingConnection.Reconnecting;
            
            // === HANDLE BREAKAGE ===

            if (intendedDisconnection)
            {
                isRetrying = false;
                runningRequest?.Dispose();
                runningRequest = null;
                connectionState = BroadcastingConnection.Disconnected;
                yield break;
            }

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