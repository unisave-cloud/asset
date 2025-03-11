using System.Collections;
using NUnit.Framework;
using Unisave.EmailAuthentication;
using Unisave.Facets;
using Unisave.Testing;
using Unisave.Utils;
using UnisaveFixture.Modules.EmailAuthentication.Backend;
using UnityEngine.TestTools;

namespace UnisaveFixture.Modules.EmailAuthentication
{
    public class EmailRegisterTest : FullstackFixture
    {
        protected override string[] BackendFolders => new string[] {
            "Assets/Plugins/Unisave/Modules/EmailAuthentication/Backend",
            "Assets/UnisaveFixture/Modules/EmailAuthentication/Backend"
        };
        
        [UnityTest]
        public IEnumerator PlayerCanRegister()
            => Asyncize.UnityTest(async () =>
        {
            EmailRegisterResponse response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Register("JOHN@doe.com", "password", true)
            );
            
            Assert.AreEqual(EmailRegisterStatusCode.Success, response.StatusCode);
            Assert.IsNotNull(response.PlayerId);

            PlayerEntity player = await caller.DB_QueryFirst<PlayerEntity>(
                @"FOR player IN players LIMIT 1 RETURN player"
            );
            Assert.IsNotNull(player);
            Assert.AreEqual("john@doe.com", player.email);
            Assert.IsTrue(Hash.Check("password", player.password));
            Assert.AreEqual(player.EntityId, response.PlayerId);
        });
        
        [UnityTest]
        public IEnumerator EmailCanBeTaken()
            => Asyncize.UnityTest(async () =>
        {
            var player = new PlayerEntity {
                email = "john@doe.com",
                password = "secret"
            };
            await caller.Entity_Save(player);
            
            EmailRegisterResponse response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Register("JOHN@doe.com", "password", true)
            );
        
            Assert.AreEqual(EmailRegisterStatusCode.EmailTaken, response.StatusCode);
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.PlayerId);

            var players = await caller.DB_QueryGet<PlayerEntity>(
                @"FOR player IN players RETURN player"
            );
            Assert.AreEqual(1, players.Count);

            await caller.Entity_Refresh(player);
            Assert.AreEqual("john@doe.com", player.email);
            Assert.AreEqual("secret", player.password);
        });
        
        [UnityTest]
        public IEnumerator InvalidEmailIsRejected()
            => Asyncize.UnityTest(async () =>
        {
            EmailRegisterResponse response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Register("john_doe.com", "password", true)
            );
    
            Assert.AreEqual(EmailRegisterStatusCode.InvalidEmail, response.StatusCode);
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.PlayerId);
            
            PlayerEntity player = await caller.DB_QueryFirst<PlayerEntity>(
                @"FOR player IN players LIMIT 1 RETURN player"
            );
            Assert.IsNull(player);
        });
        
        [UnityTest]
        public IEnumerator PasswordCannotBeWeak()
            => Asyncize.UnityTest(async () =>
        {
            EmailRegisterResponse response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Register("john@doe.com", "", true)
            );

            Assert.AreEqual(EmailRegisterStatusCode.WeakPassword, response.StatusCode);
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.PlayerId);
        
            PlayerEntity player = await caller.DB_QueryFirst<PlayerEntity>(
                @"FOR player IN players LIMIT 1 RETURN player"
            );
            Assert.IsNull(player);
        });
        
        [UnityTest]
        public IEnumerator LegalTermsHaveToBeAccepted()
            => Asyncize.UnityTest(async () =>
        {
            EmailRegisterResponse response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Register("john@doe.com", "password", false)
            );

            Assert.AreEqual(EmailRegisterStatusCode.LegalConsentRequired, response.StatusCode);
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.PlayerId);
    
            PlayerEntity player = await caller.DB_QueryFirst<PlayerEntity>(
                @"FOR player IN players LIMIT 1 RETURN player"
            );
            Assert.IsNull(player);
        });
    }
}