using System.Collections;
using NUnit.Framework;
using Unisave.Authentication;
using Unisave.Authentication.Middleware;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Sessions;
using Unisave.Testing;
using UnisaveFixture.Backend.Core.Authentication;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Core.Authentication
{
    public class AuthenticateSessionTest : BackendTestCase
    {
        [Test]
        public void ItLoadsNull()
        {
            var returned = OnFacet<AuthStubFacet>.CallSync<AuthStubPlayer>(
                nameof(AuthStubFacet.GetAuthenticatedPlayer)
            );
            
            Assert.IsNull(returned);
        }
        
        [Test]
        public void ItLoadsAuthenticatedPlayer()
        {
            var player = new AuthStubPlayer { Name = "John" };
            player.Save();
            GenerateSessionId();
            
            Session.Set(AuthenticationManager.SessionKey, player.EntityId);
            
            // HACK TO STORE THE UPDATED SESSION:
            // I need to figure out how to properly merge test facade access
            // with middleware logic so that it does not interfere.
            App.Resolve<ISession>().StoreSession(
                ClientApp.Resolve<SessionIdRepository>().GetSessionId()
            );
            
            var returned = OnFacet<AuthStubFacet>.CallSync<AuthStubPlayer>(
                nameof(AuthStubFacet.GetAuthenticatedPlayer)
            );
            
            Assert.AreEqual("John", returned?.Name);
        }

        [Test]
        public void ItStoresAuthenticatedPlayer()
        {
            var player = new AuthStubPlayer { Name = "John" };
            player.Save();
            
            Assert.IsNull(
                Session.Get<string>(AuthenticationManager.SessionKey)
            );
            
            OnFacet<AuthStubFacet>.CallSync(
                nameof(AuthStubFacet.Login), "John"
            );
            
            Assert.AreEqual(
                player.EntityId,
                Session.Get<string>(AuthenticationManager.SessionKey)
            );
        }

        [Test]
        public void ItStoresLogout()
        {
            var player = new AuthStubPlayer { Name = "John" };
            player.Save();
            
            Session.Set(AuthenticationManager.SessionKey, player.EntityId);
            
            OnFacet<AuthStubFacet>.CallSync(
                nameof(AuthStubFacet.Logout)
            );
            
            Assert.IsNull(
                Session.Get<string>(AuthenticationManager.SessionKey)
            );
        }

        [Test]
        public void ItPreservesAuthenticatedUserFromTestCase()
        {
            var player = new AuthStubPlayer { Name = "John" };
            player.Save();
            GenerateSessionId();

            ActingAs(player);
            var returned = OnFacet<AuthStubFacet>.CallSync<AuthStubPlayer>(
                nameof(AuthStubFacet.GetAuthenticatedPlayer)
            );
            Assert.AreEqual(player.EntityId, returned?.EntityId);
            
            ActingAs(null);
            returned = OnFacet<AuthStubFacet>.CallSync<AuthStubPlayer>(
                nameof(AuthStubFacet.GetAuthenticatedPlayer)
            );
            Assert.IsNull(returned);
        }
    }
}