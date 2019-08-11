using System.Collections;
using System.Collections.Generic;
using Unisave;

namespace Unisave.Examples.Chess
{
    public class PlayerEntity : Entity
    {
        /// <summary>
        /// Number of games played
        /// </summary>
        [X] public int GamesPlayed { get; set; } = 0;

        /// <summary>
        /// Number of games won
        /// </summary>
        [X] public int GamesWon { get; set; } = 0;

        /// <summary>
        /// Loads or creates this entity belonging to a certain player
        /// </summary>
        public static PlayerEntity OfPlayer(UnisavePlayer player)
        {
            var loaded = GetEntity<PlayerEntity>.OfPlayer(player).First();

            // create id needed
            if (loaded == null)
            {
                loaded = new PlayerEntity();
                loaded.Owners.Add(player);
                loaded.Save();
            }

            return loaded;
        }
    }
}
