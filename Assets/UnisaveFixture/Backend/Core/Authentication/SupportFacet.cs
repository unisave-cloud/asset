using Unisave.Facades;
using Unisave.Facets;

namespace UnisaveFixture.Backend.Core.Authentication
{
    public class SupportFacet : Facet
    {
        public void CreatePlayer(PlayerEntity player)
        {
            player.Save();
        }
    }
}