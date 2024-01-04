using System.Collections;
using NUnit.Framework;
using Unisave.EmailAuthentication;
using Unisave.Facets;
using Unisave.Testing;
using Unisave.Utils;
using UnisaveFixture.EmailAuthentication.Backend;
using UnityEngine.TestTools;

namespace UnisaveFixture.EmailAuthentication
{
    [TestFixture]
    public class EmailLoginTest : FullstackFixture
    {
        protected override string[] BackendFolders => new string[] {
            "Assets/Plugins/Unisave/Modules/EmailAuthentication/Backend",
            "Assets/UnisaveFixture/EmailAuthentication/Backend"
        };
        
        [UnityTest]
        public IEnumerator UnknownCredentialsGetRejected()
            => Asyncize.UnityTest(async () =>
        {
            var response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Login("john@doe.com", "secret")
            );
            
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.PlayerId);
            Assert.IsFalse(await caller.Auth_Check());
        });
        
        [UnityTest]
        public IEnumerator InvalidPasswordGetsRejected()
            => Asyncize.UnityTest(async () =>
        {
            await caller.Entity_Save(
                new PlayerEntity {
                    email = "john@doe.com",
                    password = Hash.Make("different-secret")
                }
            );
            
            var response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Login("john@doe.com", "secret")
            );
            
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.PlayerId);
            Assert.IsFalse(await caller.Auth_Check());
        });

        [UnityTest]
        public IEnumerator ProperCredentialsLogYouIn()
            => Asyncize.UnityTest(async () =>
        {
            await caller.Entity_Save(
                new PlayerEntity {
                    email = "john@doe.com",
                    password = Hash.Make("secret")
                }
            );

            var response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Login("john@doe.com", "secret")
            );

            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.PlayerId);
            Assert.IsTrue(await caller.Auth_Check());
            Assert.AreEqual(
                "john@doe.com",
                (await caller.Auth_GetPlayer<PlayerEntity>()).email
            );
        });
        
        [UnityTest]
        public IEnumerator EmailCanBeAnyCaseAndNonTrimmed()
            => Asyncize.UnityTest(async () =>
        {
            await caller.Entity_Save(
                new PlayerEntity {
                    email = "john@doe.com",
                    password = Hash.Make("secret")
                }
            );

            var response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Login("  JoHn@DoE.CoM   ", "secret")
            );

            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.PlayerId);
            Assert.IsTrue(await caller.Auth_Check());
            Assert.AreEqual(
                "john@doe.com",
                (await caller.Auth_GetPlayer<PlayerEntity>()).email
            );
        });
        
        [UnityTest]
        public IEnumerator RegisteredAnyCaseEmailCanLogInWhenExact()
            => Asyncize.UnityTest(async () =>
        {
            await caller.Entity_Save(
                new PlayerEntity {
                    email = "JoHn@DoE.CoM",
                    password = Hash.Make("secret")
                }
            );

            var response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Login("JoHn@DoE.CoM", "secret")
            );

            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.PlayerId);
            Assert.IsTrue(await caller.Auth_Check());
            Assert.AreEqual(
                "JoHn@DoE.CoM",
                (await caller.Auth_GetPlayer<PlayerEntity>()).email
            );
        });
        
        [UnityTest]
        public IEnumerator LoginUpdatesTimestamp()
            => Asyncize.UnityTest(async () =>
        {
            var player = new PlayerEntity
            {
                email = "john@doe.com",
                password = Hash.Make("secret")
            };
            await caller.Entity_Save(player);

            var response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Login("john@doe.com", "secret")
            );
            Assert.IsTrue(response.Success);
            Assert.AreEqual(player.EntityId, response.PlayerId);
            
            var timeBefore = player.lastLoginAt;
            await caller.Entity_Refresh(player);
            var timeNow = player.lastLoginAt;
            Assert.AreNotEqual(timeBefore, timeNow);
        });
        
        [UnityTest]
        public IEnumerator YouCanLogOut()
            => Asyncize.UnityTest(async () =>
        {
            var player = new PlayerEntity
            {
                email = "john@doe.com",
                password = Hash.Make("secret")
            };
            await caller.Entity_Save(player);

            await caller.Auth_Login(player.EntityId);

            bool response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Logout()
            );
            Assert.IsTrue(response);
            Assert.IsFalse(await caller.Auth_Check());
        });
        
        [UnityTest]
        public IEnumerator YouCanLogOutWhileNotLoggedIn()
            => Asyncize.UnityTest(async () =>
        {
            bool response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Logout()
            );
            
            Assert.IsFalse(response);
            Assert.IsFalse(await caller.Auth_Check());
        });
    }
}