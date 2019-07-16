using System;
using System.Collections.Generic;
using System.Reflection;
using RSG;
using LightJson;
using Unisave;
using Unisave.Serialization;

namespace Unisave.Facets
{
    public class EmulatedFacetCaller : FacetCaller
    {
        private UnisavePlayer caller;

        public EmulatedFacetCaller(UnisavePlayer authorizedPlayer)
        {
            this.caller = authorizedPlayer;
        }

        protected override IPromise<JsonValue> PerformFacetCall(
            string facetName, string methodName, JsonArray arguments
        )
		{
            // get all types and find the target facet type and method

            List<Type> allTypes = new List<Type>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                allTypes.AddRange(asm.GetTypes());

            Type facetType = Facet.FindFacetTypeByName(facetName, allTypes.ToArray());

            MethodInfo methodInfo = Facet.FindFacetMethodByName(facetType, methodName);

            // deserialize arguments

            object[] deserializedArguments = new object[arguments.Count];
            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length != arguments.Count)
                throw new UnisaveException(
                    $"Method '{facetName}.{methodName}' accepts different number of arguments than provided. "
                    + "Make sure you don't use the params keyword or default argument values, "
                    + "since it is not supported by Unisave."
                );

            for (int i = 0; i < arguments.Count; i++)
                deserializedArguments[i] = Loader.Load(arguments[i], parameters[i].ParameterType);

            // create facet instance and call the method

            Facet instance = Facet.CreateInstance(facetType, caller);

            object returnValue = methodInfo.Invoke(instance, deserializedArguments);

            JsonValue returnValueJson = Saver.Save(returnValue);

            // return and resolve the promise
			var promise = new Promise<JsonValue>();
			promise.Resolve(returnValueJson);
			return promise;

            // NOTE: the promise doesn't need to be rejected, since any exception will
            // kill this execution synchronously

            // However think about this in the future. The emulated server should behave
            // the same way the real server would.
		}
    }
}
