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
        public bool LoggedIn { get { return false; } }

        private LocalDatabase database;

        public LocalBackend(LocalDatabase database)
        {
            this.database = database;
        }

        public bool Login(Action success, Action<LoginFailure> failure, string email, string password)
        {
            UnityEngine.Debug.Log("Login called!");
            return false;
        }

        public bool Logout()
        {
            return false;
        }

        public bool Register(Action success, Action<RegistrationFailure> failure, string email, string password)
        {
            return false;
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
