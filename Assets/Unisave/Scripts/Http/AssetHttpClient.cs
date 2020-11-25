using System;
using System.Collections.Generic;
using LightJson;
using Unisave.Foundation;
using Unisave.Http.Client;
using UnityEngine;

namespace Unisave.Http
{
    /// <summary>
    /// Http client for the Unisave asset
    /// (the server-side one cannot be used, because it's synchronous)
    /// </summary>
    public class AssetHttpClient
    {
        private ClientApplication app;
        
        public AssetHttpClient(ClientApplication app)
        {
            this.app = app;
        }
        
        private HttpClientComponent ResolveComponent()
        {
            if (app.InEditMode)
            {
                // create a game object for the lifetime of the request
                // and then destroy it immediately
                var go = new GameObject(
                    "UnisaveHttpClient",
                    typeof(HttpClientComponent)
                );
                var component = go.GetComponent<HttpClientComponent>();
                component.DestroyImmediateAfterOneRequest = true;
                return component;
            }
            else
            {
                return app.GameObject.GetComponent<HttpClientComponent>();
            }
        }
        
        public void Get(string url, Action<Response> callback)
        {
            Send("GET", url, null, null, callback);
        }
        
        public void Post(string url, JsonObject payload, Action<Response> callback)
        {
            Send("POST", url, null, payload, callback);
        }

        private void Send(
            string method,
            string url,
            Dictionary<string, string> headers,
            JsonObject payload,
            Action<Response> callback
        )
        {
            ResolveComponent().SendRequest(
                method, url, headers, payload, callback
            );
        }
    }
}