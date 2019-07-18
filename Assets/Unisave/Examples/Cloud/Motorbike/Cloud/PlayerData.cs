using System;
using Unisave;

namespace Unisave.Examples.Cloud.Motorbike
{
    /// <summary>
    /// Represents data belonging to a player
    /// </summary>
    public class PlayerData : Entity
    {
        public string MotorbikeName { get; set; } = "Default bike name";
    }
}
