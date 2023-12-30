using System;
using Unisave.EmailAuthentication;
using Unisave.Facades;

namespace UnisaveFixture.EmailAuthentication.Backend
{
    public class EmailAuthBootstrapper : EmailAuthBootstrapperBase
    {
        public override string PlayersCollection => "players";
        public override string EmailField => "email";
        public override string PasswordField => "password";
        
        public override string RegisterNewPlayer(
            string email,
            string password
        )
        {
            var player = new PlayerEntity {
                email = email,
                password = password
            };
            
            player.Save();

            return player.EntityId;
        }
        
        public override void PlayerHasLoggedIn(string documentId)
        {
            var player = DB.Find<PlayerEntity>(documentId);
            
            player.lastLoginAt = DateTime.UtcNow;
            
            player.Save();
        }
        
        public override bool IsPasswordStrong(string password)
        {
            if (!base.IsPasswordStrong(password))
                return false;
        
            if (password.Length < 8)
                return false;
        
            return true;
        }

        public override void AssertDatabaseCollections()
        {
            // assert with each request, because the test fixture clears
            // the database with each test 
            collectionsAlreadyAsserted = false;
            
            base.AssertDatabaseCollections();
        }
    }
}