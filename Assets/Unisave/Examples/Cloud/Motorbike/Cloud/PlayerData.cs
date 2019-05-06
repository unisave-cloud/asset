using System;
using Unisave.Framework;

namespace Unisave.Examples.Cloud.Motorbike
{
    /// <summary>
    /// Represents data belonging to a player
    /// </summary>
    public class PlayerData : PlayerSingleton
    {
        public string MotorbikeName { get; set; } = "Default bike name";
    }
}
