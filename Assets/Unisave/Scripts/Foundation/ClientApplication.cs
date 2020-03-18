using System;
using Unisave.Arango;
using Unisave.Facets;
using Unisave.Sessions;
using Unisave.Utils;

namespace Unisave.Foundation
{
    /// <summary>
    /// Contains the entire client application
    /// </summary>
    public class ClientApplication : Container
    {
        /// <summary>
        /// The latest preferences that should be used by the system
        /// </summary>
        public UnisavePreferences Preferences { get; private set; }
        
        public ClientApplication(UnisavePreferences preferences)
        {
            RegisterIndependentServices();
            
            SetPreferences(preferences);
        }

        public void SetPreferences(UnisavePreferences preferences)
        {
            Preferences = preferences;
            
            // TODO: load preferences / propagate event to services
            // Re-register dependant services
        }

        /// <summary>
        /// Registers services that need not be recreated
        /// when preferences change.
        /// </summary>
        private void RegisterIndependentServices()
        {
            Bind<ApiUrl>(_ => new ApiUrl(Preferences.ServerUrl));
            
            Singleton<SessionIdRepository>(_ => new SessionIdRepository());
            
            Bind<ArangoRepository>(_ => ArangoRepository.GetInstance());
            
            Bind<FacetCaller>(_ => {
                if (Preferences.AlwaysEmulate)
                    return Resolve<EmulatedFacetCaller>();
                
                return Resolve<UnisaveFacetCaller>();
            });

            Singleton<EmulatedFacetCaller>(_ => new EmulatedFacetCaller(this));
            
            Singleton<UnisaveFacetCaller>(_ => new UnisaveFacetCaller(this));
        }
        
        #region "Singleton management"

        private static ClientApplication singletonInstance;

        public static ClientApplication GetInstance()
        {
            if (singletonInstance == null)
                singletonInstance = new ClientApplication(
                    UnisavePreferences.LoadOrCreate()
                );
            
            return singletonInstance;
        }

        public static void ForgetInstance()
        {
            singletonInstance = null;
        }
        
        #endregion
    }
}