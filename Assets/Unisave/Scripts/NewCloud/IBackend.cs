using System;
using System.Collections;
using System.Collections.Generic;
using Unisave.Framework;

namespace Unisave
{
    /// <summary>
    /// Represents the server backend
    /// Keeps connection state information
    /// </summary>
    public interface IBackend
    {
        /// <summary>
		/// If true, we have an authorized player session and we can make requests
		/// </summary>
		bool LoggedIn { get; }

        /// <summary>
        /// The logged-in player
        /// </summary>
        Player Player { get; }

        /// <summary>
		/// Starts the login coroutine
		/// </summary>
		/// <returns>False if the login request was ignored for some reason</returns>
        bool Login(Action success, Action<LoginFailure> failure, string email, string password);

        /// <summary>
		/// Starts the logout coroutine or does nothing if already logged out
		/// <returns>False if the logout request was ignored for some reason</returns>
		/// </summary>
		bool Logout();

        /// <summary>
		/// Starts the player registration coroutine
		/// </summary>
		/// <returns>False if the registration request was ignored for some reason</returns>
        bool Register(Action success, Action<RegistrationFailure> failure, string email, string password);

        /// <summary>
        /// Calls an action on the server
        /// </summary>
        void CallAction(Type controller, string action, object[] arguments);

        /// <summary>
        /// Requests entitites from the server via a query
        /// </summary>
        void RequestEntity<T>(EntityQuery query, Action<IList<T>> callback) where T : Entity, new();
    }

    /// <summary>
    /// More ways how to call certain methods
    /// </summary>
    public static class IBackendExtensions
    {
        public static bool Login(
            this IBackend backend, ILoginCallback callback, string email, string password
        )
        {
            Action success = null;
			Action<LoginFailure> failure = null;
			
			if (callback != null)
			{
				success = callback.LoginSucceeded;
				failure = callback.LoginFailed;
			}

			return backend.Login(success, failure, email, password);
        }

        public static bool Register(
            this IBackend backend, IRegistrationCallback callback, string email, string password
        )
        {
            Action success = null;
			Action<RegistrationFailure> failure = null;
			
			if (callback != null)
			{
				success = callback.RegistrationSucceeded;
				failure = callback.RegistrationFailed;
			}

			return backend.Register(success, failure, email, password);
        }
    }
}
