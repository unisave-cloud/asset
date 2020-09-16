using System.Collections;
using Unisave.Authentication;
using Unisave.Facades;
using UnisaveFixture.Backend.Core;
using UnisaveFixture.Backend.Core.Authentication;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace UnisaveFixture.Tests.Core.Authentication
{
    public class AuthenticateMiddlewareTest
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return OnFacet<FullstackUtilsFacet>.Call(
                nameof(FullstackUtilsFacet.ClearDatabase)
            ).AsCoroutine();
            
            yield return OnFacet<SupportFacet>.Call(
                nameof(SupportFacet.CreatePlayer),
                new PlayerEntity {
                    name = "John"
                }
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator GuardedMethodThrowsIfNotLoggedIn()
        {
            yield return OnFacet<AuthenticationFacet>.Call(
                nameof(AuthenticationFacet.GuardedMethod)
            ).Then(() => {
                Assert.Fail("Exception wasn't thrown.");
            }).Catch(e => {
                Assert.IsInstanceOf<AuthException>(e);
            }).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator GuardedMethodWorksIfLoggedIn()
        {
            yield return OnFacet<AuthenticationFacet>.Call(
                nameof(AuthenticationFacet.Login),
                "John"
            ).AsCoroutine();
            
            yield return OnFacet<AuthenticationFacet>.Call(
                nameof(AuthenticationFacet.GuardedMethod)
            ).AsCoroutine();
        }
    }
}