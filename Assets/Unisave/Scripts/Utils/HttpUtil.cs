using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine.Networking;
using LightJson;
using LightJson.Serialization;
using RSG;

namespace Unisave.Utils
{
    /// <summary>
    /// Encapsulates http requests into promises
    /// </summary>
    public static class HttpUtil
    {
        /// <summary>
        /// When true, request times are logged into the console
        /// </summary>
        public static bool LogRequestDuration { get; set; }
        
        /// <summary>
        /// Makes a JSON POST request and expects a JSON response.
        ///
        /// Treats any JSON response as a success. Exception is thrown only:
        /// - when there was a network problem (connection problem, etc.)
        /// - when the response is not a valid JSON
        /// - when the HTTP response status code is not one
        ///     of the listed, accepted status codes
        /// </summary>
        /// <param name="url">Absolute URL to make the request against</param>
        /// <param name="payload">Payload for the request</param>
        /// <param name="acceptCodes">
        /// Comma separated list of accepted status codes, e.g. "200,404".
        /// When you want to accept all codes, omit the parameter or set null
        /// </param>
        public static IPromise<JsonValue> Post(
            string url,
            JsonObject payload,
            string acceptCodes = null
        )
        {
            var promise = new Promise<JsonValue>();

            long[] acceptLongCodes = null;
            if (acceptCodes != null)
            {
                acceptLongCodes = acceptCodes
                    .Split(',')
                    .Select(long.Parse)
                    .ToArray();
            }

            var coroutineRunner = CoroutineRunnerComponent.GetInstance();
            var coroutine = PostCoroutine(
                url,
                payload,
                acceptLongCodes,
                promise
            );
            coroutineRunner.StartCoroutine(coroutine);

            return promise;
        }

        private static IEnumerator PostCoroutine(
            string url,
            JsonObject payload,
            long[] acceptCodes,
            Promise<JsonValue> promise
        )
        {
            HttpResponse response;
            
            // PUT because POST does not work with json for some reason
            // https://forum.unity.com/threads/posting-json-through-unitywebrequest.476254/
            using (var request = UnityWebRequest.Put(url, payload.ToString()))
            {
                // === prepare the request ===

                payload["_method"] = "POST"; // at least fake the POST method
                
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");

                // IMPORTANT: reduces latency substantially
                request.useHttpContinue = false;

                // === measure and perform the request ===

                Stopwatch stopwatch = null;
                if (LogRequestDuration)
                {
                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                }

                yield return request.SendWebRequest();

                if (LogRequestDuration && stopwatch != null)
                {
                    stopwatch.Stop();
                    UnityEngine.Debug.Log(
                        stopwatch.ElapsedMilliseconds + " ms to handle: " + url
                    );
                }
                
                // === extract the response and dispose request ===
                
                response = new HttpResponse(request);
            }

            // === check for network errors ===

            if (response.IsNetworkError)
			{
                promise.Reject(
                    new HttpException(
                        HttpException.ExceptionType.NetworkError,
                        response
                    )
                );
				yield break;
			}
            
            // === check status code ===

            if (acceptCodes != null)
            {
                if (!acceptCodes.Contains(response.StatusCode))
                {
                    promise.Reject(
                        new HttpException(
                            HttpException.ExceptionType.InvalidStatusCode,
                            response
                        )
                    );
                    yield break;
                }
            }
            
            // === parse JSON response ===

            JsonValue jsonResponse;

            try
            {
                jsonResponse = JsonReader.Parse(response.TextContent);
            }
            catch (JsonParseException)
            {
                promise.Reject(
                    new HttpException(
                        HttpException.ExceptionType.JsonParseException,
                        response
                    )
                );
				yield break;
            }

            promise.Resolve(jsonResponse);
        }
    }

    /// <summary>
    /// Pulls data from a UnityWebRequest so that it can be disposed,
    /// but the data will stay available after it's gone
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// Textual content of the response
        /// </summary>
        public string TextContent { get; }
        
        /// <summary>
        /// What HTTP status code has been returned
        /// </summary>
        public long StatusCode { get; }
        
        /// <summary>
        /// Was there a network error
        /// </summary>
        public bool IsNetworkError { get; }
        
        /// <summary>
        /// Error message describing the problem
        /// </summary>
        public string ErrorMessage { get; }
        
        public HttpResponse(UnityWebRequest request)
        {
            TextContent = request.downloadHandler.text;
            StatusCode = request.responseCode;
            IsNetworkError = request.isNetworkError;
            ErrorMessage = request.error;
        }
    }

    /// <summary>
    /// Exception thrown by the Http utility
    /// </summary>
    public class HttpException : Exception
    {
        public enum ExceptionType
        {
            /// <summary>
            /// There was a problem with connection to the server
            /// </summary>
            NetworkError,
            
            /// <summary>
            /// Returned status code was not one of the accepted ones
            /// </summary>
            InvalidStatusCode,

            /// <summary>
            /// The response wasn't valid json
            /// </summary>
            JsonParseException,
        }

        public ExceptionType Type { get; }

        public HttpResponse Response { get; }

        public HttpException(ExceptionType type, HttpResponse response)
        {
            Type = type;
            Response = response;
        }

        public override string ToString()
        {
            return "Http request failed. Failure type: " + Type + "\n"
                   + Response.ErrorMessage;
        }
    }
}
