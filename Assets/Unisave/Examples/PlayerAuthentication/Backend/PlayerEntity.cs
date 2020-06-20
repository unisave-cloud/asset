using Unisave.Entities;

namespace Unisave.Examples.PlayerAuthentication.Backend
{
    public class PlayerEntity : Entity
    {
        /// <summary>
        /// Email of the player
        /// </summary>
        public string email;

        /// <summary>
        /// Hashed password
        /// </summary>
        public string password;
    }
}
