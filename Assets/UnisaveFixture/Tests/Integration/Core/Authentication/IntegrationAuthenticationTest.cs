using System.Collections;
using NUnit.Framework;
using Unisave.Authentication;
using Unisave.Authentication.Middleware;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Sessions;
using Unisave.Testing;
using UnisaveFixture.Backend.Core.Authentication;

namespace UnisaveFixture.Tests.Core.Authentication
{
    [Ignore("Test end-to-end, like heapstore")]
    public class IntegrationAuthenticationTest : BackendTestCase
    {
        [Test]
        public void ItLoadsNull()
        {
            var returned = OnFacet<AuthenticationFacet>.CallSync<PlayerEntity>(
                nameof(AuthenticationFacet.GetPlayer)
            );
            
            Assert.IsNull(returned);
        }
        
        [Test]
        public void ItLoadsAuthenticatedPlayer()
        {
            var player = new PlayerEntity { name = "John" };
            player.Save();
            GenerateSessionId();
            
            Session.Set(AuthenticationManager.SessionKey, player.EntityId);
            
            // HACK TO STORE THE UPDATED SESSION:
            // I need to figure out how to properly merge test facade access
            // with middleware logic so that it does not interfere.
            App.Services.Resolve<ISession>().StoreSession(
                ClientApp.Services.Resolve<ClientSessionIdRepository>().GetSessionId()
            );
            
            var returned = OnFacet<AuthenticationFacet>.CallSync<PlayerEntity>(
                nameof(AuthenticationFacet.GetPlayer)
            );
            
            Assert.AreEqual("John", returned?.name);
        }

        [Test]
        public void ItStoresAuthenticatedPlayer()
        {
            var player = new PlayerEntity { name = "John" };
            player.Save();
            
            Assert.IsNull(
                Session.Get<string>(AuthenticationManager.SessionKey)
            );
            
            OnFacet<AuthenticationFacet>.CallSync(
                nameof(AuthenticationFacet.Login), "John"
            );
            
            Assert.AreEqual(
                player.EntityId,
                Session.Get<string>(AuthenticationManager.SessionKey)
            );
        }

        [Test]
        public void ItStoresLogout()
        {
            var player = new PlayerEntity { name = "John" };
            player.Save();
            
            Session.Set(AuthenticationManager.SessionKey, player.EntityId);
            
            OnFacet<AuthenticationFacet>.CallSync(
                nameof(AuthenticationFacet.Logout)
            );
            
            Assert.IsNull(
                Session.Get<string>(AuthenticationManager.SessionKey)
            );
        }

        [Test]
        public void ItPreservesAuthenticatedUserFromTestCase()
        {
            var player = new PlayerEntity { name = "John" };
            player.Save();
            GenerateSessionId();

            ActingAs(player);
            var returned = OnFacet<AuthenticationFacet>.CallSync<PlayerEntity>(
                nameof(AuthenticationFacet.GetPlayer)
            );
            Assert.AreEqual(player.EntityId, returned?.EntityId);
            
            ActingAs(null);
            returned = OnFacet<AuthenticationFacet>.CallSync<PlayerEntity>(
                nameof(AuthenticationFacet.GetPlayer)
            );
            Assert.IsNull(returned);
        }
    }
}