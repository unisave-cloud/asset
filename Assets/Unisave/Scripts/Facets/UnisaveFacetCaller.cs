using System;
using System.Runtime.Serialization;
using LightJson;
using RSG;
using Unisave.Utils;
using Unisave.Exceptions;
using Unisave.Foundation;
using Unisave.Logging;
using Unisave.Serialization;
using UnityEngine;
using Application = UnityEngine.Application;

namespace Unisave.Facets
{
    public class UnisaveFacetCaller : FacetCaller
    {
		private readonly ClientApplication app;

        public UnisaveFacetCaller(ClientApplication app) : base(app)
        {
	        this.app = app;
        }

		protected override IPromise<JsonValue> PerformFacetCall(
            string facetName,
            string methodName,
            JsonArray arguments
        )
		{
			var promise = new Promise<JsonValue>();

			Http.Post(
				app.Resolve<ApiUrl>().CallFacet(),
				new JsonObject()
					.Add("facetName", facetName)
					.Add("methodName", methodName)
					.Add("arguments", arguments)
					.Add("sessionId", SessionId)
					.Add("deviceId", DeviceIdRepository.GetDeviceId())
					.Add("device", DeviceIdRepository.GetDeviceInfo())
					.Add("gameToken", app.Preferences.GameToken)
					.Add("client", new JsonObject()
						.Add("backendHash", app.Preferences.BackendHash)
						.Add("frameworkVersion", FrameworkMeta.Version)
						.Add("assetVersion", AssetMeta.Version)
						.Add("buildGuid", Application.buildGUID)
						.Add("versionString", Application.version)
					),
				"200"
			).Then(response => {
				JsonObject executionResult = response["executionResult"];

				JsonObject specialValues = executionResult["special"].AsJsonObject
				                           ?? new JsonObject();
				
				// remember the session id
				string givenSessionId = specialValues["sessionId"].AsString;
				if (givenSessionId != null)
					SessionId = givenSessionId;
				
				// print logs
				LogPrinter.PrintLogsFromFacetCall(specialValues["logs"]);
				
				switch (executionResult["result"].AsString)
				{
					case "ok":
						promise.Resolve(executionResult["returned"]);
						break;

					case "exception":
						var e = Serializer.FromJson<Exception>(
							executionResult["exception"]
						);
						PreserveStackTrace(e);
						promise.Reject(e);
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
		
		// magic
		// https://stackoverflow.com/a/2085377
		private static void PreserveStackTrace(Exception e)
		{
			var ctx = new StreamingContext(StreamingContextStates.CrossAppDomain);
			var mgr = new ObjectManager(null, ctx);
			var si = new SerializationInfo(e.GetType(), new FormatterConverter());

			e.GetObjectData(si, ctx);
			mgr.RegisterObject(e, 1, si); // prepare for SetObjectData
			mgr.DoFixups(); // ObjectManager calls SetObjectData

			// voila, e is unmodified save for _remoteStackTraceString
		}
    }
}
