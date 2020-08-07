using System;
using Unisave.Facades;
using Unisave.Facets;

namespace UnisaveFixture.Backend.Core.Authentication
{
    public class AuthStubFacet : Facet
    {
        public AuthStubPlayer GetAuthenticatedPlayer()
        {
            return Auth.GetPlayer<AuthStubPlayer>();
        }

        public void Login(string name)
        {
            var player = DB.TakeAll<AuthStubPlayer>()
                .Filter(entity => entity.Name == name)
                .First();
            
            if (player == null)
                throw new ArgumentException("Invalid player name");
            
            Auth.Login(player);
        }

        public void Logout()
        {
            Auth.Logout();
        }
    }
}