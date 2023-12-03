using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NUnit.Framework;
using Unisave.Facets;
using UnityEngine;

namespace UnisaveFixture.Tests.Unit
{
    [TestFixture]
    public class JWTTest
    {
        [Test]
        public void ItLoadsJwtLibrary()
        {
            // from the framework
            var serializer = new Unisave.JWT.LightJsonSerializer();

            var header = new JWT.Builder.JwtHeader();

            Assert.IsTrue(true);
        }
    }
}