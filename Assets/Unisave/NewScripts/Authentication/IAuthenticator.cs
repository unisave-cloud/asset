using System;
using System.Collections;
using System.Collections.Generic;
using RSG;

namespace Unisave.Authentication
{
    /// <summary>
    /// Handles player authnetication
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Logged in player, or null
        /// </summary>
        UnisavePlayer Player { get; }

        /// <summary>
        /// Is some player logged in
        /// </summary>
        bool LoggedIn { get; }

        /// <summary>
        /// What access token to use, when talking with the server when authenticated
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Attempts to login a player
        /// </summary>
        IPromise Login(string email, string password);

        /// <summary>
        /// Attempts to logout a player
        /// </summary>
        IPromise Logout();

        /// <summary>
        /// Attempts to register a new player
        /// </summary>
        IPromise Register(string email, string password, Dictionary<string, object> hookArguments);
    }

    ///////////////////////////
    // Login data structures //
    ///////////////////////////
    
    public class LoginFailure : Exception
    {
        public LoginFailureType type;
        public string message;

        public override string ToString()
        {
            return "Login failure type: " + type.ToString() + "\n" + message;
        }
    }

    public enum LoginFailureType
    {
        OK,
        NetworkError,
        InvalidGameToken,
        InvalidCredentials,
        PlayerBanned,
        AlreadyLoggedIn,
        ServerUnderMaintenance,
        GameClientOutdated,
        OtherError
    }

    ////////////////////////////
    // Logout data structures //
    ////////////////////////////
    
    public class LogoutFailure : Exception
    {
        public LogoutFailureType type;
        public string message;

        public override string ToString()
        {
            return "Logout failure type: " + type.ToString() + "\n" + message;
        }
    }

    public enum LogoutFailureType
    {
        NetworkError,
        NotLoggedIn,
        OtherError
    }

    //////////////////////////////////
    // Registration data structures //
    //////////////////////////////////

    public class RegistrationFailure : Exception
    {
        public RegistrationFailureType type;
        public string message;

        public override string ToString()
        {
            return "Registration failure type: " + type.ToString() + "\n" + message;
        }
    }

    public enum RegistrationFailureType
    {
        NetworkError,
        InvalidGameToken,
        EmailAlreadyRegistered,
        InvalidEmail,
        InvalidPassword,
        ServerUnderMaintenance,
        OtherError
    }
}
