using System;
using Unisave;
using Unisave.Authentication.Middleware;
using Unisave.Facades;
using Unisave.Facets;

namespace UnisaveFixture.Backend.Core.Authentication
{
    public class AuthenticationFacet : Facet
    {
        public PlayerEntity GetPlayer()
        {
            return Auth.GetPlayer<PlayerEntity>();
        }
        
        public bool Check()
        {
            return Auth.Check();
        }
        
        public string Id()
        {
            return Auth.Id();
        }

        public void Login(string name)
        {
            var player = DB.TakeAll<PlayerEntity>()
                .Filter(entity => entity.name == name)
                .First();
            
            if (player == null)
                throw new ArgumentException("Invalid player name");
            
            Auth.Login(player);
        }

        public void Logout()
        {
            Auth.Logout();
        }

        [Middleware(typeof(Authenticate))]
        public void GuardedMethod()
        {
            Log.Info("Guarded method has been called!");
        }
    }
}