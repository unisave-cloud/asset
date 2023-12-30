using System;
using Unisave.Entities;

namespace UnisaveFixture.EmailAuthentication.Backend
{
    [EntityCollectionName("players")]
    public class PlayerEntity : Entity
    {
        public string email;
        public string password;
        public DateTime lastLoginAt;
    }
}