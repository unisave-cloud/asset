using System;
using Unisave.Facades;
using Unisave.Facets;
using Unisave.Utils;

namespace Unisave.Examples.PlayerAuthentication.Backend
{
    /// <summary>
    /// Handles player login and registration
    /// </summary>
    public class AuthFacet : Facet
    {
        /// <summary>
        /// Tries to log the player in
        /// </summary>
        /// <param name="email">Player's email</param>
        /// <param name="password">Player's password</param>
        /// <returns>True when the login succeeds</returns>
        public bool Login(string email, string password)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));
            
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            
            var player = FindPlayer(email);

            if (player == null)
                return false;

            if (!Hash.Check(password, player.password))
                return false;

            Auth.Login(player);
            return true;
        }

        /// <summary>
        /// Logs the player out
        /// </summary>
        /// <returns>
        /// False if the player wasn't logged in to begin with.
        /// </returns>
        public bool Logout()
        {
            bool wasLoggedIn = Auth.Check();
            Auth.Logout();
            return wasLoggedIn;
        }
        
        /// <summary>
        /// Registers a new player
        /// </summary>
        /// <param name="email">Player's email</param>
        /// <param name="password">Player's password</param>
        public RegistrationResult Register(string email, string password)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));
            
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            
            string normalizedEmail = NormalizeEmail(email);

            if (!IsEmailValid(normalizedEmail))
                return RegistrationResult.InvalidEmail;
            
            if (!IsPasswordStrong(password))
                return RegistrationResult.WeakPassword;
            
            if (FindPlayer(email) != null)
                return RegistrationResult.EmailTaken;
            
            // register
            var player = new PlayerEntity {
                email = normalizedEmail,
                password = Hash.Make(password)
            };
            player.Save();
            
            // login
            Auth.Login(player);
            
            return RegistrationResult.Ok;
        }

        /// <summary>
        /// Determines whether the given password is strong enough.
        /// </summary>
        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;
            
            // you can add additional constraints here
            
            return true;
        }

        /// <summary>
        /// Normalizes email address (trim + lowercase).
        /// Use this method when storing and finding email addresses
        /// in the database to make the process seem case-insensitive.
        /// </summary>
        public static string NormalizeEmail(string email)
        {
            return email?.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Checks that the given string is a valid email address.
        /// </summary>
        public static bool IsEmailValid(string email)
        {
            try
            {
                var parsed = new System.Net.Mail.MailAddress(email);
                return parsed.Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// Finds a player by the email address in the same way
        /// login method finds the player. Returns null if no player was found.
        /// </summary>
        public static PlayerEntity FindPlayer(string email)
        {
            // find as-is
            // (allows old players with invalid email addresses to login)
            var player = DB.TakeAll<PlayerEntity>()
                .Filter(entity => entity.email == email)
                .First();

            if (player != null)
                return player;
            
            // find with normalized email address
            // (the default login method)
            player = DB.TakeAll<PlayerEntity>()
                .Filter(entity => entity.email == NormalizeEmail(email))
                .First();

            return player;
        }
        
        /// <summary>
        /// Encodes all possible results of player registration
        /// </summary>
        public enum RegistrationResult
        {
            Ok = 0,
            InvalidEmail = 1,
            WeakPassword = 2,
            EmailTaken = 3,
        }
    }
}
