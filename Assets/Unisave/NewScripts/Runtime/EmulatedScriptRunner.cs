using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LightJson;
using LightJson.Serialization;
using Unisave.Serialization;
using Unisave.Exceptions;
using Unisave.Database;

namespace Unisave.Runtime
{
    /// <summary>
    /// Runs scripts directly through the framework entrypoint to make sure
    /// script emulation behaves in exactly the same way as real script execution
    /// </summary>
    public static class EmulatedScriptRunner
    {
        public static ScriptExecutionResult ExecuteScript(string method, JsonObject methodParameters)
        {
            if (!(Endpoints.Database is EmulatedDatabase))
                throw new UnisaveException(
                    "Emulating script execution, but framework database endpoint is not an emulated database."
                );

            var db = (EmulatedDatabase)Endpoints.Database;

            JsonObject executionParameters = new JsonObject()
                .Add("executionId", "emulated-execution")
                .Add("databaseProxyIp", JsonValue.Null) // database endpoint is already setup
                .Add("databaseProxyPort", 0)
                .Add("executionMethod", method)
                .Add("methodParameters", methodParameters);

            List<Type> allTypes = new List<Type>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                allTypes.AddRange(asm.GetTypes());

            db.PreventAccess = false;

            string responseString = Entrypoint.Start(executionParameters.ToString(), allTypes.ToArray());
            JsonObject response = JsonReader.Parse(responseString);

            db.PreventAccess = true;

            switch (response["result"].AsString)
            {
                case "ok":
                    return ScriptExecutionResult.Ok(response["methodResponse"]);

                case "exception":
                    return ScriptExecutionResult.Exception(
                        (Exception) Serializer.FromJson(response["exception"], typeof(Exception))
                    );
                
                case "invalid-method-parameters":
                    return ScriptExecutionResult.InvalidMethodParameters(response["message"]);
                
                case "error":
                    return ScriptExecutionResult.Error(response["errorMessage"]);
            }

            throw new UnisaveException(
                $"Script execution resulted with unknown result code {response["result"].AsString}."
            );
        }
    }
}
