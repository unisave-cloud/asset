using System;
using System.Collections.Generic;
using System.Reflection;
using RSG;
using LightJson;
using Unisave;
using Unisave.Serialization;
using Unisave.Database;
using Unisave.Exceptions;
using Unisave.Runtime;

namespace Unisave.Facets
{
    public class EmulatedFacetCaller : FacetCaller
    {
        private Func<UnisavePlayer> GetAuthorizedPlayer;

        public EmulatedFacetCaller(Func<UnisavePlayer> GetAuthorizedPlayer)
        {
            this.GetAuthorizedPlayer = GetAuthorizedPlayer;
        }

        protected override IPromise<JsonValue> PerformFacetCall(
            string facetName, string methodName, JsonArray arguments
        )
		{
            ScriptExecutionResult result = EmulatedScriptRunner.ExecuteScript(
                "facet",
                new JsonObject()
                    .Add("facetName", facetName)
                    .Add("methodName", methodName)
                    .Add("arguments", arguments)
                    .Add("callerId", GetAuthorizedPlayer().Id)
            );

            if (result.IsOK)
            {
                return Promise<JsonValue>.Resolved(
                    result.methodResponse["returnValue"]
                );
            }
            else
            {
                return Promise<JsonValue>.Rejected(
                    result.TransformNonOkResultToFinalException()
                );
            }
		}
    }
}
