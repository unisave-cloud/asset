using System;
using Unisave.Foundation;

namespace Unisave.Facades
{
    /// <summary>
    /// Represents all client-side facades
    /// </summary>
    public static class ClientFacade
    {
        /// <summary>
        /// Application instance that should be used by facades
        /// </summary>
        public static ClientApplication ClientApp => clientApp
            ?? throw new InvalidOperationException(
             "Trying to use a facade, but facades have not been "
             + "initialized to an application instance."
            );

        private static ClientApplication clientApp;
        
        /// <summary>
        /// True if an application instance is set and can be used
        /// </summary>
        public static bool HasApp => clientApp != null;
        
        /// <summary>
        /// Sets the application instance to be used by facades
        /// </summary>
        public static void SetApplication(ClientApplication newApp)
        {
            clientApp = newApp;
        }
    }
}