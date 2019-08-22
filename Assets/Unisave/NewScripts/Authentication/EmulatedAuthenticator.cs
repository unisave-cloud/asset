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

        /// <summary>
        /// Allows access to the database for a given window of time
        /// </summary>
        private Action<Action> DatabaseAccessWindow;

        public EmulatedAuthenticator(Func<EmulatedDatabase> GetDatabase, Action<Action> DatabaseAccessWindow)
        {
            this.GetDatabase = GetDatabase;
            this.DatabaseAccessWindow = DatabaseAccessWindow;
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

            // update database
            if (database.EnumeratePlayers().Where(r => r.email == email).Any())
            {
                return Promise.Rejected(new RegistrationFailure {
                    type = RegistrationFailureType.EmailAlreadyRegistered,
                    message = "This email is already registered."
                });
            }

            string playerId = database.AddPlayer(email);
            UnisavePlayer player = new UnisavePlayer(playerId);

            // execute hooks
            List<Type> allTypes = new List<Type>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                allTypes.AddRange(asm.GetTypes());

            if (hookArguments == null)
                hookArguments = new Dictionary<string, object>();
                
            var jsonHookArguments = new Dictionary<string, JsonValue>();
            foreach (var pair in hookArguments)
                jsonHookArguments.Add(pair.Key, Serializer.ToJson(pair.Value));

            List<PlayerRegistrationHook> hooks = allTypes
                .Where(t => typeof(PlayerRegistrationHook).IsAssignableFrom(t))
                .Where(t => t != typeof(PlayerRegistrationHook)) // not the abstract hook class itself
                .Select(t => PlayerRegistrationHook.CreateInstance(t, player, jsonHookArguments))
                .ToList();

            hooks.Sort((a, b) => a.Order - b.Order); // small to big

            JsonArray jsonArguments = Serializer.ToJson(hookArguments.ToList());

            DatabaseAccessWindow(() => {
                foreach (var hook in hooks)
                    hook.Run();
            });

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
