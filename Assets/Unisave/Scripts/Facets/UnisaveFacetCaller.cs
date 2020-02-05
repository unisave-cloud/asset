using System;
using UnityEngine;
using LightJson;
using RSG;
using Unisave.Utils;
using Unisave.Exceptions;
using Unisave.Serialization;

namespace Unisave.Facets
{
    public class UnisaveFacetCaller : FacetCaller
    {
	    /// <summary>
	    /// Session ID that is used for communication with the server
	    /// </summary>
	    public string SessionId { get; private set; }
	    
		private readonly ApiUrl apiUrl;
		private readonly string gameToken;

        public UnisaveFacetCaller(ApiUrl apiUrl, string gameToken)
        {
            this.apiUrl = apiUrl;
            this.gameToken = gameToken;
        }

		protected override IPromise<JsonValue> PerformFacetCall(
            string facetName, string methodName, JsonArray arguments
        )
		{
			var promise = new Promise<JsonValue>();

			Http.Post(
				apiUrl.CallFacet(),
				new JsonObject()
					.Add("facetName", facetName)
					.Add("methodName", methodName)
					.Add("arguments", arguments)
					.Add("sessionId", SessionId)
					.Add("gameToken", gameToken)
					.Add("client", new JsonObject()
						.Add("backendHash",
							UnisavePreferences.LoadOrCreate().BackendHash)
						.Add("frameworkVersion", FrameworkMeta.Version)
						.Add("assetVersion", AssetMeta.Version)
						.Add("buildGuid", Application.buildGUID)
						.Add("versionString", Application.version)
					),
				"200"
			).Then(response => {
				JsonObject executionResult = response["executionResult"];
				// TODO: pull out logs and stuff
				
				// remember the session id
				string givenSessionId = executionResult["special"]["sessionId"];
				if (givenSessionId != null)
					SessionId = givenSessionId;
				
				switch (executionResult["result"].AsString)
				{
					case "ok":
						promise.Resolve(executionResult["returned"]);
						break;

					case "exception":
						promise.Reject(
							Serializer.FromJson<Exception>(
								executionResult["exception"]
							)
						);
						break;
					
					default:
						promise.Reject(
							new UnisaveException(
								"Server sent unknown response for facet call:\n"
								+ response
							)
						);
						break;
				}
				
			}).Catch(e => {
				promise.Reject(e);
			});

			return promise;
		}
    }
}
