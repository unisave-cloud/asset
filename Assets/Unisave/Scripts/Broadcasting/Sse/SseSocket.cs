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
        /// The event ID that's sent when we aren't reconnecting
        /// </summary>
        public const int NullEventId = -1;
        
        /// <summary>
        /// Called when a new event arrives
        /// </summary>
        public event Action<SseEvent> OnEventReceived;

        private UnityWebRequest runningRequest;

        private ClientApplication app;
        
        public int lastReceivedEventId = NullEventId;
        public int retryMilliseconds = 15_000;
        public bool isRetrying = false;
        public BroadcastingConnection connectionState
            = BroadcastingConnection.Disconnected;

        private bool intendedDisconnection = false;

        /// <summary>
        /// Call this right after this component is created
        /// </summary>
        public void Initialize(ClientApplication app)
        {
            this.app = app;
            lastReceivedEventId = NullEventId;
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

            var downloadHandler = new SseDownloadHandler(HandleEvent);
            
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
                            ["lastReceivedEventId"] = lastReceivedEventId
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
            AppendToDebugLog(
                $"CONNECTING, lastReceivedEventId: {lastReceivedEventId}\n\n"
            );
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

        private void HandleEvent(SseEvent @event)
        {
            if (@event.id != null)
                lastReceivedEventId = (int) @event.id;

            if (@event.retry != null)
                retryMilliseconds = (int) @event.retry;
            
            OnEventReceived?.Invoke(@event);
        }
    }
}