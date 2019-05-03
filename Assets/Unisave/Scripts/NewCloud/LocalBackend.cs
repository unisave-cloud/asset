using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public LocalBackend(LocalDatabase database)
        {
            this.database = database;
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

        public void CallAction(string action, params object[] arguments)
        {

        }

        public void RequestEntity<T>(EntityQuery query, Action<IEnumerable<T>> callback) where T : Entity, new()
        {
            IEnumerable<T> entities = database.RunEntityQuery<T>(query);
            callback.Invoke(entities);
        }
    }
}
