using System;
using Unisave.Entities;

namespace UnisaveFixture.Backend.Authentication
{
    /*
     * NOTE: There is no PlayerEntity template. The entity has to be
     * created manually via instructions in the Unisave documentation.
     * That is because the entity is shared between many templates.
     */
    
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
        
        /// <summary>
        /// Last time the player logged in
        /// </summary>
        public DateTime lastLoginAt = DateTime.UtcNow;
    }
}