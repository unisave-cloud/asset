using System;
using System.Collections;
using System.Diagnostics;
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
using Debug = System.Diagnostics.Debug;

namespace Unisave.Facets
{
    public class UnisaveFacetCaller : FacetCaller
    {
		private Func<string> GetAccessToken;
		private ApiUrl apiUrl;
        private CoroutineRunnerComponent coroutineRunner;

        public UnisaveFacetCaller(
	        Func<string> GetAccessToken,
	        ApiUrl apiUrl,
	        CoroutineRunnerComponent coroutineRunner
	    )
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

			Http.Post(
				apiUrl.CallFacet(),
				new JsonObject()
					.Add("access_token", GetAccessToken())
					.Add("facet", facetName)
					.Add("method", methodName)
					.Add("arguments", arguments),
				"200"
			).Then(response => {
				switch (response["result"].AsString)
				{
					case "ok":
						if (response["hasReturnValue"].AsBoolean)
							promise.Resolve(response["returnValue"]);
						else
							promise.Resolve(JsonValue.Null);
						break;

					case "game-exception":
						promise.Reject(
							Serializer.FromJson<Exception>(
								response["exception"]
							)
						);
						break;

					case "error":
						promise.Reject(
							new UnisaveException(
								"Facet call error:\n" + response["message"]
							)
						);
						break;

					// DEPRECATED
					case "compile-error":
						promise.Reject(
							new UnisaveException(
								"Server compile error:\n" + response["message"]
							)
						);
						break;
					
					default:
						promise.Reject(
							new UnisaveException(
								"Server sent unknown result for facet call."
							)
						);
						break;
				}
			}).Catch(e => {
				if (!(e is HttpException))
				{
					promise.Reject(e);
					return;
				}

				HttpException he = (HttpException)e;
				
				if (he.Response.StatusCode == 401)
				{
					promise.Reject(
						new InvalidAccessTokenException(
							"Unisave server rejected facet call, " +
							"because the provided access token was invalid."
						)
					);
					return;
				}
				
				promise.Reject(e);
			});

			return promise;
		}
    }
}
