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
    public class EmailRegisterTest : FullstackFixture
    {
        protected override string[] BackendFolders => new string[] {
            "Assets/Plugins/Unisave/Modules/EmailAuthentication/Backend",
            "Assets/UnisaveFixture/EmailAuthentication/Backend"
        };
        
        [UnityTest]
        public IEnumerator PlayerCanRegister()
            => Asyncize.UnityTest(async () =>
        {
            EmailRegisterResponse response = await caller.CallFacet(
                (EmailAuthFacet f) => f.Register("JOHN@doe.com", "password")
            );
            
            Assert.AreEqual(EmailRegisterResponse.Ok, response);

            PlayerEntity player = await caller.DB_QueryFirst<PlayerEntity>(
                @"FOR player IN players LIMIT 1 RETURN player"
            );
            Assert.IsNotNull(player);
            Assert.AreEqual("john@doe.com", player.email);
            Assert.IsTrue(Hash.Check("password", player.password));
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
                (EmailAuthFacet f) => f.Register("JOHN@doe.com", "password")
            );
        
            Assert.AreEqual(EmailRegisterResponse.EmailTaken, response);

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
                (EmailAuthFacet f) => f.Register("john_doe.com", "password")
            );
    
            Assert.AreEqual(EmailRegisterResponse.InvalidEmail, response);
            
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
                (EmailAuthFacet f) => f.Register("john@doe.com", "")
            );

            Assert.AreEqual(EmailRegisterResponse.WeakPassword, response);
        
            PlayerEntity player = await caller.DB_QueryFirst<PlayerEntity>(
                @"FOR player IN players LIMIT 1 RETURN player"
            );
            Assert.IsNull(player);
        });
    }
}