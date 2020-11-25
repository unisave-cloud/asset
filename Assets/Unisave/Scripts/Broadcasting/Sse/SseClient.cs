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
    
    public class SseClient : MonoBehaviour
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

        #region "Debug log"
        #if UNITY_EDITOR
        
        public bool displayDebugLog = true; // TODO: false

        private StringBuilder debugLog = new StringBuilder();

        void AppendToDebugLog(string text)
        {
            const int maxLength = 1024 * 10;
            
            debugLog.Append(text);

            if (debugLog.Length > maxLength)
                debugLog.Remove(0, debugLog.Length - maxLength);
        }
        
        void OnGUI()
        {
            if (!displayDebugLog)
                return;

            GUI.Label(
                new Rect(20, 20, Screen.width - 40, Screen.height - 40),
                debugLog.ToString(),
                new GUIStyle(GUI.skin.textArea) {
                    alignment = TextAnchor.LowerLeft
                }
            );
        }
        
        #endif
        #endregion

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