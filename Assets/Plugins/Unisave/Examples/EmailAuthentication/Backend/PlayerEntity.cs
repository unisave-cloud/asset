using System;
using Unisave.Entities;

namespace Unisave.Examples.EmailAuthentication.Backend
{
    [EntityCollectionName("players")]
    public class PlayerEntity : Entity
    {
        /// <summary>
        /// Email of the player
        /// </summary>
        [DontLeaveServer]
        public string email;
        
        /// <summary>
        /// Hashed password of the player
        /// </summary>
        [DontLeaveServer]
        public string password;
        
        /// <summary>
        /// When was the last time the player has logged in
        /// </summary>
        public DateTime lastLoginAt = DateTime.UtcNow;

        /// <summary>
        /// Example player data - number of collected stars.
        /// This can be: coins, level progression, inventory, etc...
        /// </summary>
        public int collectedStars = 0;
    }
}
