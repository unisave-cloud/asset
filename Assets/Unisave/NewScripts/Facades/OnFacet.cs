using System;
using Unisave;
using RSG;

namespace Unisave
{
    /// <summary>
    /// Facade for calling facet methods on the server
    /// </summary>
    /// <typeparam name="F">Target facet class</typeparam>
    public static class OnFacet<F> where F : Facet
    {
        /// <summary>
        /// Calls a facet method that has a return value
        /// </summary>
        /// <param name="methodName">Name of the facet method</param>
        /// <param name="arguments">Arguments for the method</param>
        /// <typeparam name="R">Return type of the method</typeparam>
        /// <returns>Promise that will resolve once the call finishes</returns>
        public static IPromise<R> Call<R>(string methodName, params object[] arguments)
        {
            return UnisaveServer.DefaultInstance.FacetCaller.CallFacetMethod<F, R>(methodName, arguments);
        }

        /// <summary>
        /// Calls a facet method that returns void
        /// </summary>
        /// <param name="methodName">Name of the facet method</param>
        /// <param name="arguments">Arguments for the method</param>
        /// <returns>Promise that will resolve once the call finishes</returns>
        public static IPromise Call(string methodName, params object[] arguments)
        {
            return UnisaveServer.DefaultInstance.FacetCaller.CallFacetMethod<F>(methodName, arguments);
        }
    }
}
