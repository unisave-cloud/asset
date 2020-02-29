using System;
using System.Collections.Generic;
using RSG;
using LightJson;
using Unisave.Arango;
using Unisave.Arango.Emulation;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Foundation;
using Unisave.Runtime;
using Unisave.Runtime.Kernels;
using Unisave.Sessions;

namespace Unisave.Facets
{
    public class EmulatedFacetCaller : FacetCaller
    {
        /// <summary>
        /// Session instance that will be used by the application
        /// </summary>
        public SessionOverStorage Session { get; private set; }
        
        private readonly ClientApplication clientApp;

        /// <summary>
        /// Arango in-memory database used by the server application
        /// </summary>
        private ArangoInMemory arango;
        
        public EmulatedFacetCaller(ClientApplication clientApp)
        {
            this.clientApp = clientApp;
            
            Session = new SessionOverStorage(
                new InMemorySessionStorage(),
                3600
            );
        }

        protected override IPromise<JsonValue> PerformFacetCall(
            string facetName,
            string methodName,
            JsonArray arguments
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
            
            SaveDatabase();
            
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

        private void PerformContainerSurgery(Application serverApp)
        {
            // replace session instance
            serverApp.Instance<ISession>(Session);

            // replace the database instance
            var arangoRepo = clientApp.Resolve<ArangoRepository>();
            arango = arangoRepo.GetDatabase(
                clientApp.Preferences.EmulatedDatabaseName
            );
            serverApp.Instance<IArango>(arango);
        }

        private void SaveDatabase()
        {
            var arangoRepo = clientApp.Resolve<ArangoRepository>();
            arangoRepo.SaveDatabase(
                clientApp.Preferences.EmulatedDatabaseName,
                arango // assigned in PerformContainerSurgery(...)
            );
        }
    }
}
