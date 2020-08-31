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
    public class EmailRegisterTest : BackendTestCase
    {
        [Test]
        public void PlayerCanRegister()
        {
            var response = OnFacet<EmailRegisterFacet>.CallSync<EmailRegisterResponse>(
                nameof(EmailRegisterFacet.Register),
                "JOHN@doe.com",
                "password"
            );
            
            Assert.AreEqual(EmailRegisterResponse.Ok, response);

            var player = DB.First<PlayerEntity>();
            Assert.IsNotNull(player);
            Assert.AreEqual("john@doe.com", player.email);
            Assert.IsTrue(Hash.Check("password", player.password));
        }

        [Test]
        public void EmailCanBeTaken()
        {
            var player = new PlayerEntity {
                email = "john@doe.com",
                password = "secret"
            };
            player.Save();
            
            var response = OnFacet<EmailRegisterFacet>.CallSync<EmailRegisterResponse>(
                nameof(EmailRegisterFacet.Register),
                "JOHN@doe.com",
                "password"
            );
            
            Assert.AreEqual(EmailRegisterResponse.EmailTaken, response);
            
            Assert.AreEqual(
                1,
                DB.TakeAll<PlayerEntity>().Get().Count
            );
            player.Refresh();
            Assert.AreEqual("john@doe.com", player.email);
            Assert.AreEqual("secret", player.password);
        }

        [Test]
        public void InvalidEmailIsRejected()
        {
            var response = OnFacet<EmailRegisterFacet>.CallSync<EmailRegisterResponse>(
                nameof(EmailRegisterFacet.Register),
                "john_doe.com",
                "password"
            );
            
            Assert.AreEqual(EmailRegisterResponse.InvalidEmail, response);
            
            var player = DB.First<PlayerEntity>();
            Assert.IsNull(player);
        }

        [Test]
        public void PasswordCannotBeWeak()
        {
            var response = OnFacet<EmailRegisterFacet>.CallSync<EmailRegisterResponse>(
                nameof(EmailRegisterFacet.Register),
                "john@doe.com",
                ""
            );
            
            Assert.AreEqual(EmailRegisterResponse.WeakPassword, response);
            
            var player = DB.First<PlayerEntity>();
            Assert.IsNull(player);
        }
    }
}