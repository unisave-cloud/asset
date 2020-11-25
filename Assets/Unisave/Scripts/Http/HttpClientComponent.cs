using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using LightJson;
using Unisave.Http.Client;
using UnityEngine;
using UnityEngine.Networking;

namespace Unisave.Http
{
    public class HttpClientComponent : MonoBehaviour
    {
        // TODO: display debug log, like SSE socket has

        /// <summary>
        /// Should the game object be destroyed after one performed request?
        /// (used for requests in edit mode)
        /// </summary>
        public bool DestroyImmediateAfterOneRequest { get; set; } = false;
        
        public void SendRequest(
            string method,
            string url,
            Dictionary<string, string> headers,
            JsonObject payload,
            Action<Response> callback
        )
        {
            StartCoroutine(
                SendRequestCoroutine(method, url, headers, payload, callback)
            );
        }
        
        private IEnumerator SendRequestCoroutine(
            string method,
            string url,
            Dictionary<string, string> headers,
            JsonObject payload,
            Action<Response> callback
        )
        {
            // TODO: enforce SSL certificates
            
            var downloadHandler = new DownloadHandlerBuffer();

            UploadHandler uploadHandler = null;
            if (payload != null)
                uploadHandler = new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(payload.ToString())
                );

            var runningRequest = new UnityWebRequest(
                url,
                method,
                downloadHandler,
                uploadHandler
            );
            
            if (headers != null)
                foreach (var pair in headers)
                    runningRequest.SetRequestHeader(pair.Key, pair.Value);
            
            if (payload != null)
                runningRequest.SetRequestHeader("Content-Type", "application/json");

            yield return runningRequest.SendWebRequest();

            var contentType = runningRequest.GetResponseHeader("Content-Type")
                ?? "text/plain";

            var response = Response.Create(
                downloadHandler.text,
                new ContentType(contentType).Name,
                (int)runningRequest.responseCode
            );
            
            callback?.Invoke(response);

            if (DestroyImmediateAfterOneRequest)
                DestroyImmediate(gameObject);
        }
    }
}