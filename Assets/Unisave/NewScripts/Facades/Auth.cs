using System.Collections;
using System.Collections.Generic;
using RSG;

namespace Unisave
{
    /// <summary>
    /// Facade for player authentication
    /// </summary>
    public static class Auth
    {
        /// <summary>
        /// Logged in player or null
        /// </summary>
        public static UnisavePlayer Player => UnisaveServer.DefaultInstance.Authenticator.Player;

        /// <summary>
        /// Is someone logged in?
        /// </summary>
        public static bool LoggedIn => UnisaveServer.DefaultInstance.Authenticator.LoggedIn;

        /// <summary>
        /// Attempt player login
        /// </summary>
        public static IPromise Login(string email, string password)
        {
            return UnisaveServer.DefaultInstance.Authenticator.Login(email, password);
        }

        /// <summary>
        /// Attempt to logout current player
        /// </summary>
        public static IPromise Logout()
        {
            return UnisaveServer.DefaultInstance.Authenticator.Logout();
        }

        /// <summary>
        /// Attempt to register a new player
        /// </summary>
        public static IPromise Register(string email, string password, Dictionary<string, object> hookArguments = null)
        {
            return UnisaveServer.DefaultInstance.Authenticator.Register(email, password, hookArguments);
        }
    }
}
