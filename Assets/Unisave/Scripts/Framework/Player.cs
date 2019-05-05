using System;

namespace Unisave.Framework
{
    /// <summary>
    /// Represents a player of your game
    /// </summary>
    public class Player
    {
        public string ID { get; private set; }

        public Player(string id)
        {
            this.ID = id;
        }
    }
}
