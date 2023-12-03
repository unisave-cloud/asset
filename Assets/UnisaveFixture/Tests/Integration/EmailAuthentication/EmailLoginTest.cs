using System.Collections;
using NUnit.Framework;
using Unisave.Facades;
using Unisave.Testing;
using Unisave.Utils;
using UnisaveFixture.Backend.Authentication;
using UnisaveFixture.Backend.EmailAuthentication;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.EmailAuthentication
{
    [Ignore("Extract into separate asset and test end-to-end, like heapstore")]
    public class EmailLoginTest : BackendTestCase
    {
        [Test]
        public void UnknownCredentialsGetRejected()
        {
            var response = OnFacet<EmailLoginFacet>.CallSync<bool>(
                nameof(EmailLoginFacet.Login),
                "john@doe.com",
                "secret"
            );
            
            Assert.IsFalse(response);
            Assert.IsFalse(Auth.Check());
        }

        [Test]
        public void InvalidPasswordGetsRejected()
        {
            new PlayerEntity {
                email = "john@doe.com",
                password = Hash.Make("different-secret")
            }.Save();
            
            var response = OnFacet<EmailLoginFacet>.CallSync<bool>(
                nameof(EmailLoginFacet.Login),
                "john@doe.com",
                "secret"
            );
            
            Assert.IsFalse(response);
            Assert.IsFalse(Auth.Check());
        }

        [Test]
        public void ProperCredentialsLogYouIn()
        {
            new PlayerEntity {
                email = "john@doe.com",
                password = Hash.Make("secret")
            }.Save();
            
            var response = OnFacet<EmailLoginFacet>.CallSync<bool>(
                nameof(EmailLoginFacet.Login),
                "john@doe.com",
                "secret"
            );
            
            Assert.IsTrue(response);
            Assert.IsTrue(Auth.Check());
            Assert.AreEqual(
                "john@doe.com",
                Auth.GetPlayer<PlayerEntity>().email
            );
        }

        [Test]
        public void EmailCanBeAnyCaseAndNonTrimmed()
        {
            new PlayerEntity {
                email = "john@doe.com",
                password = Hash.Make("secret")
            }.Save();
            
            var response = OnFacet<EmailLoginFacet>.CallSync<bool>(
                nameof(EmailLoginFacet.Login),
                "  JoHn@DoE.CoM   ",
                "secret"
            );
            
            Assert.IsTrue(response);
            Assert.IsTrue(Auth.Check());
            Assert.AreEqual(
                "john@doe.com",
                Auth.GetPlayer<PlayerEntity>().email
            );
        }

        [Test]
        public void RegisteredAnyCaseEmailCanLogInWhenExact()
        {
            new PlayerEntity {
                email = "JoHn@DoE.CoM",
                password = Hash.Make("secret")
            }.Save();
            
            var response = OnFacet<EmailLoginFacet>.CallSync<bool>(
                nameof(EmailLoginFacet.Login),
                "JoHn@DoE.CoM",
                "secret"
            );
            
            Assert.IsTrue(response);
            Assert.IsTrue(Auth.Check());
            Assert.AreEqual(
                "JoHn@DoE.CoM",
                Auth.GetPlayer<PlayerEntity>().email
            );
        }
        
        [Test]
        public void LoginUpdatesTimestamp()
        {
            var player = new PlayerEntity {
                email = "john@doe.com",
                password = Hash.Make("secret")
            };
            player.Save();
            
            OnFacet<EmailLoginFacet>.CallSync<bool>(
                nameof(EmailLoginFacet.Login),
                "john@doe.com",
                "secret"
            );

            var timeBefore = player.lastLoginAt;
            player.Refresh();
            var timeNow = player.lastLoginAt;
            Assert.AreNotEqual(timeBefore, timeNow);
        }

        [Test]
        public void YouCanLogOut()
        {
            var player = new PlayerEntity {
                email = "john@doe.com",
                password = Hash.Make("secret")
            };
            player.Save();

            ActingAs(player);
            
            var response = OnFacet<EmailLoginFacet>.CallSync<bool>(
                nameof(EmailLoginFacet.Logout)
            );
            
            Assert.IsTrue(response);
            Assert.IsFalse(Auth.Check());
        }
        
        [Test]
        public void YouCanLogOutWhileNotLoggedIn()
        {
            var response = OnFacet<EmailLoginFacet>.CallSync<bool>(
                nameof(EmailLoginFacet.Logout)
            );
            
            Assert.IsFalse(response);
            Assert.IsFalse(Auth.Check());
        }
    }
}