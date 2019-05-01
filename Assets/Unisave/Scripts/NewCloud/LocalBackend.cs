using System;

namespace Unisave
{
    /// <summary>
    /// Backend for debugging
    /// Data is stored locally, no internet communication happens
    /// </summary>
    public class LocalBackend : IBackend
    {
        public bool LoggedIn { get { return false; } }

        public bool Login(Action success, Action<LoginFailure> failure, string email, string password)
        {
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

        public void RequestEntity(/* ? */)
        {

        }
    }
}
