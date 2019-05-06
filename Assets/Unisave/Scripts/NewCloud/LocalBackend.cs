using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unisave.Framework;
using Unisave.Framework.Endpoints;

namespace Unisave
{
    /// <summary>
    /// Backend for debugging
    /// Data is stored locally, no internet communication happens
    /// </summary>
    public class LocalBackend : IBackend
    {
        public Player Player { get; private set; }

        public bool LoggedIn { get { return Player != null; } }

        private LocalDatabase database;

        private IFrameworkBase localFrameworkBase;

        public LocalBackend(LocalDatabase database, IFrameworkBase localFrameworkBase)
        {
            this.database = database;
            this.localFrameworkBase = localFrameworkBase;
        }

        public bool Login(Action success, Action<LoginFailure> failure, string email, string password)
        {
            if (LoggedIn)
                Logout();

            var player = database.players.Where(x => x.email == email).FirstOrDefault();

            if (player == null)
            {
                if (failure != null)
                    failure.Invoke(new LoginFailure {
                        type = LoginFailure.FailureType.BadCredentials,
                        message = "Player with such email does not exist."
                    });

                return true;
            }

            Player = new Player(player.id);
            
            if (success != null)
                success.Invoke();

            return true;
        }

        public bool Logout()
        {
            if (!LoggedIn)
                return false;

            Player = null;
            return true;
        }

        public bool Register(Action success, Action<RegistrationFailure> failure, string email, string password)
        {
            var player = database.players.Where(x => x.email == email).FirstOrDefault();

            if (player != null)
            {
                if (failure != null)
                    failure.Invoke(new RegistrationFailure {
                        type = RegistrationFailure.FailureType.EmailAlreadyRegistered,
                        message = "There already exists a player with this email."
                    });

                return true;
            }

            database.players.Add(new LocalDatabase.PlayerRecord {
                id = "ID_" + email,
                email = email
            });

            database.Save();

            if (success != null)
                success.Invoke();

            return true;
        }

        public void CallAction(Type controller, string action, object[] arguments)
        {
            if (!LoggedIn)
                throw new InvalidOperationException(
                    "Cannot call controller actions without being logged in."
                );

            StaticBase.OverrideBase(localFrameworkBase, () => {
                var endpoint = new CallActionEndpoint(localFrameworkBase);
                endpoint.CallAction(controller, action, Player, arguments);
            });
        }

        public void RequestEntity<T>(EntityQuery query, Action<IList<T>> callback) where T : Entity, new()
        {
            if (!LoggedIn)
                throw new InvalidOperationException(
                    "Cannot request entities without being logged in."
                );

            IList<T> entities = null;

            StaticBase.OverrideBase(localFrameworkBase, () => {
                var endpoint = new RequestEntityEndpoint(localFrameworkBase);
                entities = endpoint.RequestEntity<T>(query);
            });

            callback.Invoke(entities);
        }
    }
}
