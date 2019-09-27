using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using LightJson;
using LightJson.Serialization;
using RSG;
using Unisave.Utils;
using Unisave.Exceptions;
using Unisave.Exceptions.ServerConnection;
using Unisave.Serialization;

namespace Unisave.Facets
{
    public class UnisaveFacetCaller : FacetCaller
    {
		private Func<string> GetAccessToken;
		private ApiUrl apiUrl;
        private CoroutineRunnerComponent coroutineRunner;

        public UnisaveFacetCaller(Func<string> GetAccessToken, ApiUrl apiUrl, CoroutineRunnerComponent coroutineRunner)
        {
			this.GetAccessToken = GetAccessToken;
            this.coroutineRunner = coroutineRunner;
			this.apiUrl = apiUrl;
        }

		protected override IPromise<JsonValue> PerformFacetCall(
            string facetName, string methodName, JsonArray arguments
        )
		{
			var promise = new Promise<JsonValue>();

			var coroutine = CallFacetRequestCoroutine(
				promise,
                facetName,
                methodName,
                arguments
            );

            coroutineRunner.StartCoroutine(coroutine);

			return promise;
		}

        private IEnumerator CallFacetRequestCoroutine(
            Promise<JsonValue> promise, string facetName, string methodName, JsonArray arguments
        )
		{
			string payload = new JsonObject()
                .Add("access_token", GetAccessToken())
				.Add("facet", facetName)
                .Add("method", methodName)
                .Add("arguments", arguments)
				.ToString();

			// put because post does not work with json for some reason
			// https://forum.unity.com/threads/posting-json-through-unitywebrequest.476254/
			UnityWebRequest request = UnityWebRequest.Put(apiUrl.CallFacet(), payload);
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Accept", "application/json");
			
			yield return request.SendWebRequest();

			if (request.isNetworkError)
			{
				promise.Reject(
					new UnisaveException(
						"Facet call failed due to a network error: " + request.error
					)
				);
				yield break;
			}

			if (request.responseCode == 401)
			{
				promise.Reject(
					new InvalidAccessTokenException(
						"Unisave server rejected facet call, because the provided access token was invalid."
					)
				);
				yield break;
			}

			if (request.responseCode != 200)
			{
				promise.Reject(
					new UnisaveException(
						$"Unisave server didn't perform facet call, HTTP response was {request.responseCode}."
					)
				);
				yield break;
			}

			JsonObject response;
			string result;
			try
			{
				JsonValue responseValue = JsonReader.Parse(request.downloadHandler.text);
				
				if (!responseValue.IsJsonObject)
					throw new JsonParseException();
				
				response = responseValue.AsJsonObject;

				if (!response.ContainsKey("result"))
					throw new JsonParseException();

				result = response["result"].AsString;
			}
			catch (JsonParseException)
			{
				promise.Reject(new UnisaveException("Facet call failed, server response had invalid format."));
				yield break;
			}

			switch (result)
			{
				case "ok":
					if (response["hasReturnValue"].AsBoolean)
						promise.Resolve(response["returnValue"]);
					else
						promise.Resolve(JsonValue.Null);
					break;

				case "game-exception":
					Exception e = new UnisaveException(
						"Server exception: " + response["message"] // deprecated method of returning exceptions
					);
					
					if (!response["exception"].IsNull)
						e = (Exception) Serializer.FromJson(response["exception"], typeof(Exception));
					
					promise.Reject(e);
					break;

				case "error":
					promise.Reject(new UnisaveException("Facet call error:\n" + response["message"]));
					break;

				case "compile-error":
					promise.Reject(new UnisaveException("Server compile error:\n" + response["message"]));
					break;
			}
		}
    }
}
