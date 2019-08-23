using System;
using RSG;
using Unisave.Database;
using System.Linq;
using System.Collections.Generic;
using Unisave.Utils;
using Unisave.Runtime;
using Unisave.Serialization;
using LightJson;
using System.Reflection;
using Unisave.Exceptions;
using Unisave.Exceptions.PlayerRegistration;

namespace Unisave.Authentication
{
    public class EmulatedAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Password that will be rejected during login. Any other is accepted.
        /// </summary>
        public const string RejectionPassword = "reject";

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

        /// <summary>
        /// Obtains current emulated database instance
        /// </summary>
        private Func<EmulatedDatabase> GetDatabase;

        public EmulatedAuthenticator(Func<EmulatedDatabase> GetDatabase)
        {
            this.GetDatabase = GetDatabase;
        }

        /// <inheritdoc/>
        public IPromise Login(string email, string password)
		{
            EmulatedDatabase database = GetDatabase();

            string playerId = database.EnumeratePlayers()
                .Where(r => r.email == email)
                .Select(r => r.id)
                .FirstOrDefault();

            if (playerId == null)
            {
                return Promise.Rejected(new LoginFailure {
                    type = LoginFailureType.InvalidCredentials,
                    message = "Provided credentials are invalid. (emulation is active, email not found)"
                });
            }

            if (password == RejectionPassword)
            {
                return Promise.Rejected(new LoginFailure {
                    type = LoginFailureType.InvalidCredentials,
                    message = "Provided credentials are invalid. (emulation is active, password is rejecting)"
                });
            }

            // login
            Player = new UnisavePlayer(playerId);
            AccessToken = Str.Random(16);
            return Promise.Resolved();
		}

        /// <inheritdoc/>
        public IPromise Logout()
        {
            Player = null;
            AccessToken = null;
            return Promise.Resolved();
        }

        /// <inheritdoc/>
        public IPromise Register(string email, string password, Dictionary<string, object> hookArguments)
        {
            EmulatedDatabase database = GetDatabase();

            // prevent duplicity
            if (database.EnumeratePlayers().Where(r => r.email == email).Any())
                return Promise.Rejected(new EmailAlreadyRegisteredException());

            // update database
            string playerId = database.AddPlayer(email);

            // run hooks
            ScriptExecutionResult result = EmulatedScriptRunner.ExecuteScript(
                "player-registration-hook",
                new JsonObject()
                    .Add("arguments", Serializer.ToJson(hookArguments))
                    .Add("playerId", playerId)
            );

            if (!result.IsOK)
            {
                // remove player
                database.RemovePlayer(playerId);

                return Promise.Rejected(result.TransformNonOkResultToFinalException());
            }

            return Promise.Resolved();
        }

        /// <summary>
        /// Logs in the fake emulated player
        /// </summary>
        public void LoginEmulatedPlayer()
        {
            Player = EmulatedDatabase.EmulatedPlayer;
            AccessToken = "emulated-player-access-token";
        }

        /// <summary>
        /// Perform an action as a different user
        /// </summary>
        public void AsPlayer(UnisavePlayer player, Action action)
        {
            UnisavePlayer oldPlayer = Player;
            string oldAccessToken = AccessToken;

            Player = player;
            AccessToken = "access-token-of-some-random-player";

            action();

            Player = oldPlayer;
            AccessToken = oldAccessToken;
        }
    }
}
