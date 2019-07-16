using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave.Auth
{
    /// <summary>
    /// Handles player authentication against unisave servers
    /// </summary>
    public class UnisaveAuthenticator : IAuthenticator
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

        // flags showing which coroutines are currently running
        private bool loginCoroutineRunning = false;

        /// <summary>
        /// Something that can run coroutines
        /// </summary>
        private CoroutineRunnerComponent coroutineRunner;

        /// <summary>
        /// Stateles encapsulation of the actual HTTP requests needed
        /// </summary>
        private ServerAuthApi api;

        public UnisaveAuthenticator(CoroutineRunnerComponent coroutineRunner, ServerAuthApi api)
		{
			this.coroutineRunner = coroutineRunner;
			this.api = api;
		}

        public bool Login(Action success, Action<LoginFailure> failure, string email, string password)
		{
			if (loginCoroutineRunning)
			{
				Debug.LogWarning("Unisave: Trying to login while already logging in.");
				return false;
			}

			if (Player != null)
			{
				Debug.LogWarning("Unisave: Trying to login while already logged in.");
				return false;
			}

			loginCoroutineRunning = true;
			
			IEnumerator coroutine = api.Login(result => {

				loginCoroutineRunning = false;

				if (result.type == ServerAuthApi.LoginResultType.OK)
				{
					AccessToken = result.accessToken;
					Player = result.player;

					if (success != null)
						success.Invoke();
				}
				else
				{
					if (failure != null)
						failure.Invoke(new LoginFailure {
							type = LoginFailure.TypeFromApiResultType(result.type),
							message = result.message
						});
				}
				
			}, email, password);

			coroutineRunner.StartCoroutine(coroutine);

			return true;
		}
    }
}
