using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RSG;
using LightJson;
using Unisave.Exceptions;
using Unisave.Serialization;
using Unisave.Runtime;

namespace Unisave.Facets
{
    /// <summary>
    /// Handles facet calling
    /// </summary>
    public abstract class FacetCaller
    {
        /// <summary>
        /// Calls facet method that has a return value
        /// </summary>
        /// <typeparam name="F">Facet class</typeparam>
        /// <typeparam name="R">Method return type</typeparam>
        /// <returns>Promise that resolves when the call finishes</returns>
        public IPromise<R> CallFacetMethod<F, R>(
            string methodName, params object[] arguments
        )
        {
            return CallFacetMethod(typeof(F), typeof(R), methodName, arguments)
                .Then((object ret) => (R) ret);
        }

        /// <summary>
        /// Calls face method with return value in a non-generic way
        /// </summary>
        public IPromise<object> CallFacetMethod(
            Type facetType, Type returnType, string methodName, params object[] arguments
        )
        {
            // check method return type
            MethodInfo methodInfo = ExecutionHelper.FindMethodByName(facetType, methodName);
            
            if (methodInfo.ReturnType != returnType)
                throw new UnisaveException(
                    $"OnFacet<{facetType.Name}>.Call<{returnType.Name}>(\"{methodName}\", ...)"
                    + " is incorrent (method returns different type), use:\n"
                    + $"OnFacet<{facetType.Name}>.Call<{methodInfo.ReturnType.Name}>(...)"
                );

            return PerformFacetCall(facetType.Name, methodName, SerializeArguments(arguments))
                .Then((JsonValue returnedValue) => {
                    return Loader.Load(returnedValue, returnType);
                });
        }

        /// <summary>
        /// Calls facet method that returns void
        /// </summary>
        /// <typeparam name="F">Facet class</typeparam>
        /// <returns>Promise that resolves when the call finishes</returns>
        public IPromise CallFacetMethod<F>(string methodName, params object[] arguments)
        {
            return CallFacetMethod(typeof(F), methodName, arguments);
        }

        /// <summary>
        /// Calls facet method that returns void in a non-generic way
        /// </summary>
        public IPromise CallFacetMethod(
            Type facetType, string methodName, params object[] arguments
        )
        {
            // check method return type
            MethodInfo methodInfo = ExecutionHelper.FindMethodByName(facetType, methodName);
            
            if (methodInfo.ReturnType != typeof(void))
                throw new UnisaveException(
                    $"OnFacet<{facetType.Name}>.Call(\"{methodName}\", ...)"
                    + " is incorrent (method doesn't return void), use:\n"
                    + $"OnFacet<{facetType.Name}>.Call<{methodInfo.ReturnType.Name}>(...)"
                );

            return PerformFacetCall(facetType.Name, methodName, SerializeArguments(arguments))
                .Then(v => {}); // forget the return value, which is null anyways
        }

        /// <summary>
        /// Performs the actual facet call
        /// 
        /// (this does serialize aparently for no reason when emulating, but this way
        /// it checks, that serialization works and no pesky references are kept between
        /// client-side and server-side data)
        /// </summary>
        /// <returns>
        /// Either the serialized return value if it has one,
        /// or an exception if promise gets rejected.
        /// </returns>
        protected abstract IPromise<JsonValue> PerformFacetCall(
            string facetName, string methodName, JsonArray arguments
        );

        /// <summary>
        /// Serializes arguments passed to the facet method
        /// </summary>
        private JsonArray SerializeArguments(object[] arguments)
        {
            var jsonArgs = new JsonArray();
			for (int i = 0; i < arguments.Length; i++)
				jsonArgs.Add(Saver.Save(arguments[i]));
            return jsonArgs;
        }
    }
}
