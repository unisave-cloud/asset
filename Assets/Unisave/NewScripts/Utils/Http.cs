using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using LightJson;
using LightJson.Serialization;
using RSG;

namespace Unisave.Utils
{
    /// <summary>
    /// Encapsulates http requests into promises
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Make a post request to a given url with json payload and json response
        /// </summary>
        public static IPromise<JsonValue> Post(string url, JsonObject payload)
        {
            var promise = new Promise<JsonValue>();

            var coroutineRunner = CoroutineRunnerComponent.GetInstance();
            var coroutine = PostCoroutine(url, payload, promise);
            coroutineRunner.StartCoroutine(coroutine);

            return promise;
        }

        private static IEnumerator PostCoroutine(string url, JsonObject payload, Promise<JsonValue> promise)
        {
            // PUT because POST does not work with json for some reason
			// https://forum.unity.com/threads/posting-json-through-unitywebrequest.476254/
            payload["_method"] = "POST"; // at least fake it
			UnityWebRequest request = UnityWebRequest.Put(url, payload.ToString());
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.isNetworkError)
			{
                promise.Reject(new HttpException(HttpException.ExceptionType.NetworkError, request));
				yield break;
			}

            JsonValue response;

            try
            {
                response = JsonReader.Parse(request.downloadHandler.text);
            }
            catch (JsonParseException)
            {
                promise.Reject(new HttpException(HttpException.ExceptionType.JsonParseException, request));
				yield break;
            }

            promise.Resolve(response);
        }
    }

    public class HttpException : Exception
    {
        public enum ExceptionType
        {
            /// <summary>
            /// There was a problem with connection to the server
            /// </summary>
            NetworkError,

            /// <summary>
            /// The response wasn't valid json
            /// </summary>
            JsonParseException,
        }

        public ExceptionType Type { get; private set; }

        public UnityWebRequest Request { get; private set; }

        public HttpException(ExceptionType type, UnityWebRequest request) : base()
        {
            this.Type = type;
            this.Request = request;
        }

        public override string ToString()
        {
            return "Http request failed. Failure type: " + Type.ToString() + "\n" + Request.error;
        }
    }
}
