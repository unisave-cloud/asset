using System;

namespace Unisave.Framework
{
    /// <summary>
    /// Represents a player of your game
    /// </summary>
    public class Player
    {
        public string ID { get; private set; }

        /// <summary>
        /// Currently logged-in player
        /// </summary>
        public Player Me
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Player(string id)
        {
            this.ID = id;
        }
    }
}
