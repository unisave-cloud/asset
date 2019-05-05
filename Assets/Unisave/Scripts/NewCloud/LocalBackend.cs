using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unisave.Framework;

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
                return false;

            var player = database.players.Where(x => x.email == email).First();

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
            var player = database.players.Where(x => x.email == email).First();

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

            return true;
        }

        public void CallAction(Type controller, string action, params object[] arguments)
        {
            MethodInfo mi = controller.GetMethod(action);

            if (mi == null)
            {
                throw new ArgumentException(
                    "Provided controller " + controller + " lacks method: " + action,
                    nameof(controller)
                );
            }

            Controller ctrl = Controller.CreateInstance(controller, Player);
            
            // call the action within an appropriate framework base
            StaticBase.OverrideBase(localFrameworkBase, () => {
                mi.Invoke(ctrl, arguments);
            });
        }

        public void RequestEntity<T>(EntityQuery query, Action<IEnumerable<T>> callback) where T : Entity, new()
        {
            IEnumerable<T> entities = database.RunEntityQuery<T>(query);
            callback.Invoke(entities);
        }
    }
}
