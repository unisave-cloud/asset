using System;

namespace Unisave.Auth
{
    public class EmulatedAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Authenticated player
        /// </summary>
        public UnisavePlayer Player { get; private set; }

        /// <summary>
        /// Is some player logged in
        /// </summary>
        public bool LoggedIn => Player != null;

        /// <summary>
        /// Token that is used to access the server when authenticated
        /// </summary>
        public string AccessToken { get; private set; }

        public bool Login(Action success, Action<LoginFailure> failure, string email, string password)
		{
			// do some checks against the emulated player database
            return true;
		}

        /// <summary>
        /// Logs in the fake emulated player
        /// </summary>
        public void LoginEmulatedPlayer()
        {
            Player = new UnisavePlayer("emulated-player-id");
            AccessToken = "emulated-player-access-token";
        }
    }
}
