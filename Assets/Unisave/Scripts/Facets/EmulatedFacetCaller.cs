using System;
using System.Collections.Generic;
using System.Reflection;
using RSG;
using LightJson;
using Unisave;
using Unisave.Contracts;
using Unisave.Serialization;
using Unisave.Exceptions;
using Unisave.Facades;
using Unisave.Foundation;
using Unisave.Runtime;
using Unisave.Runtime.Kernels;
using Unisave.Sessions;
using Unisave.Utils;

namespace Unisave.Facets
{
    public class EmulatedFacetCaller : FacetCaller
    {
        /// <summary>
        /// Session ID that is used for communication with the server
        /// </summary>
        public string SessionId { get; private set; }
        
        /// <summary>
        /// Session instance that will be used by the application
        /// </summary>
        public SessionOverStorage Session { get; private set; }
        
        public EmulatedFacetCaller()
        {
            Session = new SessionOverStorage(
                new InMemorySessionStorage(),
                3600
            );
        }

        protected override IPromise<JsonValue> PerformFacetCall(
            string facetName, string methodName, JsonArray arguments
        )
		{
            var env = new Env();
            
            // override with additional dev configuration
            var preferences = UnisavePreferences.LoadOrCreate();
            if (preferences.DevelopmentEnv != null)
            {
                var overrideEnv = Env.Parse(preferences.DevelopmentEnv.text);
                env.OverrideWith(overrideEnv);
            }
            
            var app = Bootstrap.Boot(
                GetGameAssemblyTypes(),
                env,
                new SpecialValues()
            );
            
            Facade.SetApplication(app);

            PerformContainerSurgery(app);
            
            // BEGIN RUN THE APP
            
            var methodParameters = new FacetCallKernel.MethodParameters(
                facetName,
                methodName,
                arguments,
                SessionId
            );
                
            var kernel = app.Resolve<FacetCallKernel>();
                
            var returnedJson = kernel.Handle(methodParameters);

            var specialValues = app.Resolve<SpecialValues>();
            SessionId = specialValues.Read("sessionId").AsString;
            
            // END RUN THE APP
            
            Facade.SetApplication(null);
            
            app.Dispose();
            
            return Promise<JsonValue>.Resolved(returnedJson);
		}
        
        private Type[] GetGameAssemblyTypes()
        {
            // NOTE: gets all possible types, since there might be asm-def files
            // that make the situation more difficult
            
            List<Type> types = new List<Type>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                types.AddRange(asm.GetTypes());
            }

            return types.ToArray();
        }

        private void PerformContainerSurgery(Application app)
        {
            app.Instance<ISession>(Session);
            
            // TODO: replace database
        }
    }
}
