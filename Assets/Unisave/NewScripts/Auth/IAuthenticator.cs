using System;
using System.Collections;
using System.Collections.Generic;

namespace Unisave.Auth
{
    /// <summary>
    /// Handles player authnetication
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Logged in player, or null
        /// </summary>
        UnisavePlayer Player { get; }

        /// <summary>
        /// Is some player logged in
        /// </summary>
        bool LoggedIn { get; }

        /// <summary>
        /// What access token to use, when talking with the server when authenticated
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Attempts to login a player
        /// </summary>
        /// <param name="success">Callback on success</param>
        /// <param name="failure">Callback on failure</param>
        /// <param name="email">Player's email address</param>
        /// <param name="password">Player's password</param>
        /// <returns>False if the login request was ignored for some reason</returns>
        bool Login(Action success, Action<LoginFailure> failure, string email, string password);

        // TODO:
        // logout
        // registration
    }
}
