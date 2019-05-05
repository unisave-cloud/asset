using System;

namespace Unisave.Framework
{
    /// <summary>
    /// Entity that a player always has exactly once
    /// </summary>
    public class PlayerSingleton : PlayerEntity
    {
        public override void Create()
        {
            // if already exists, throw an exception

            base.Create();
        }
    }
}
