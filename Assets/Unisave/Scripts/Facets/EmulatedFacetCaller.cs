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
        private Func<EmulatedDatabase> GetEmulatedDatabase;

        public EmulatedFacetCaller(
            Func<UnisavePlayer> GetAuthorizedPlayer,
            Func<EmulatedDatabase> GetEmulatedDatabase
        )
        {
            this.GetAuthorizedPlayer = GetAuthorizedPlayer;
            this.GetEmulatedDatabase = GetEmulatedDatabase;
        }

        protected override IPromise<JsonValue> PerformFacetCall(
            string facetName, string methodName, JsonArray arguments
        )
		{
            ScriptExecutionResult result = EmulatedScriptRunner.ExecuteScript(
                GetEmulatedDatabase(),
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
