using System.Collections;
using System.Collections.Generic;
using Unisave;

namespace Unisave.Examples.Chess
{
    public class HomeFacet : Facet
    {
        /// <summary>
        /// Returns the entity with player data
        /// </summary>
        public PlayerEntity DownloadPlayerEntity()
        {
            return PlayerEntity.OfPlayer(Caller);
        }
    }
}
