using Unisave.Authentication;
using Unisave.Entities;

namespace Unisave.Testing
{
    public partial class BackendTestCase
    {
        /// <summary>
        /// Make the given player the authenticated player
        /// </summary>
        protected BackendTestCase ActingAs(Entity player)
        {
            var manager = App.Resolve<AuthenticationManager>();
            manager.SetPlayer(player);
            return this;
        }
    }
}