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
        /// Preferences that should be used by the application
        /// </summary>
        public UnisavePreferences Preferences { get; }
        
        public ClientApplication(UnisavePreferences preferences)
        {
            Preferences = preferences;
            
            RegisterServices();
        }

        /// <summary>
        /// Registers default services
        /// </summary>
        private void RegisterServices()
        {
            Bind<ApiUrl>(_ => new ApiUrl(Preferences.ServerUrl));
            
            Singleton<SessionIdRepository>(_ => new SessionIdRepository());
            
            Singleton<DeviceIdRepository>(_ => new DeviceIdRepository());
            
            Singleton<FacetCaller>(_ => new UnisaveFacetCaller(this));
        }
    }
}