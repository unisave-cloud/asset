using Unisave.Authentication;
using Unisave.Authentication.Middleware;
using Unisave.Contracts;
using Unisave.Entities;
using Unisave.Facades;
using Unisave.Sessions;

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
            
            // HACK TO STORE THE UPDATED SESSION:
            // I need to figure out how to properly merge test facade access
            // with middleware logic so that it does not interfere.
            Session.Set(AuthenticateSession.SessionKey, player?.EntityId);
            App.Resolve<ISession>().StoreSession(
                ClientApp.Resolve<SessionIdRepository>().GetSessionId()
            );
            
            return this;
        }
    }
}