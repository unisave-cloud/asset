using System;
using System.Collections;
using System.Collections.Generic;
using Unisave.Framework;

namespace Unisave
{
    public class CloudBackend : IBackend
    {
        public Player Player { get; private set; }

        public bool LoggedIn { get { return Player != null; } }

        public CloudBackend()
        {
            
        }

        public bool Login(Action success, Action<LoginFailure> failure, string email, string password)
        {
            Kill();
            return false;
        }

		public bool Logout()
        {
            Kill();
            return false;
        }

        public bool Register(Action success, Action<RegistrationFailure> failure, string email, string password)
        {
            Kill();
            return false;
        }

        public void CallAction(Type controller, string action, object[] arguments)
        {
            Kill();
        }

        public void RequestEntity<T>(EntityQuery query, Action<IList<T>> callback) where T : Entity, new()
        {
            Kill();
        }

        private void Kill()
        {
            throw new NotImplementedException(
                "UnisaveCloud currently works only against the local database." +
                "Go to the Unisave panel and check the option for running against local database."
            );
        }
    }
}
