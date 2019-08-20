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

        public Func<bool> PreventDatabaseAccess;
        private bool preventDatabaseAccess = true;

        public EmulatedFacetCaller(Func<UnisavePlayer> GetAuthorizedPlayer)
        {
            PreventDatabaseAccess = () => preventDatabaseAccess;

            this.GetAuthorizedPlayer = GetAuthorizedPlayer;
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

            // execute the facet
            
            Facet instance = Facet.CreateInstance(facetType, GetAuthorizedPlayer());
            JsonValue returnValue;
            
            try
            {
                preventDatabaseAccess = false;
                returnValue = ExecutionHelper.ExecuteMethod(
                    instance, methodName, arguments, out MethodInfo methodInfo
                );
            }
            catch (TargetInvocationException e)
            {
                // behave just like the real server would
                return Promise<JsonValue>.Rejected(
                    new UnisaveFacetCaller.RemoteException(e.InnerException.ToString())
                );
            }
            finally
            {
                preventDatabaseAccess = true;
            }

            return Promise<JsonValue>.Resolved(returnValue);
		}
    }
}
