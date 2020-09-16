using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unisave.Authentication;
using Unisave.Facades;
using UnisaveFixture.Backend.Core;
using UnisaveFixture.Backend.Core.Authentication;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace UnisaveFixture.Tests.Core.Authentication
{
    public class AuthFacadeTest
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
        public IEnumerator FirstThereIsNoAuthenticatedPlayer()
        {
            yield return OnFacet<AuthenticationFacet>.Call<PlayerEntity>(
                nameof(AuthenticationFacet.GetPlayer)
            ).Then(p => {
                Assert.IsNull(p);
            }).AsCoroutine();
            
            yield return OnFacet<AuthenticationFacet>.Call<bool>(
                nameof(AuthenticationFacet.Check)
            ).Then(c => {
                Assert.IsFalse(c);
            }).AsCoroutine();
            
            yield return OnFacet<AuthenticationFacet>.Call<string>(
                nameof(AuthenticationFacet.Id)
            ).Then(id => {
                Assert.IsNull(id);
            }).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator JohnCanLogIn()
        {
            yield return OnFacet<AuthenticationFacet>.Call(
                nameof(AuthenticationFacet.Login),
                "John"
            ).AsCoroutine();
            
            yield return OnFacet<AuthenticationFacet>.Call<PlayerEntity>(
                nameof(AuthenticationFacet.GetPlayer)
            ).Then(p => {
                Assert.AreEqual("John", p.name);
            }).AsCoroutine();
            
            yield return OnFacet<AuthenticationFacet>.Call<bool>(
                nameof(AuthenticationFacet.Check)
            ).Then(c => {
                Assert.IsTrue(c);
            }).AsCoroutine();
            
            yield return OnFacet<AuthenticationFacet>.Call<string>(
                nameof(AuthenticationFacet.Id)
            ).Then(id => {
                Assert.IsNotNull(id);
            }).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator JohnCanLogOut()
        {
            yield return OnFacet<AuthenticationFacet>.Call(
                nameof(AuthenticationFacet.Login),
                "John"
            ).AsCoroutine();
            
            yield return OnFacet<AuthenticationFacet>.Call(
                nameof(AuthenticationFacet.Logout)
            ).AsCoroutine();
            
            yield return OnFacet<AuthenticationFacet>.Call<PlayerEntity>(
                nameof(AuthenticationFacet.GetPlayer)
            ).Then(p => {
                Assert.IsNull(p);
            }).AsCoroutine();
            
            yield return OnFacet<AuthenticationFacet>.Call<bool>(
                nameof(AuthenticationFacet.Check)
            ).Then(c => {
                Assert.IsFalse(c);
            }).AsCoroutine();
            
            yield return OnFacet<AuthenticationFacet>.Call<string>(
                nameof(AuthenticationFacet.Id)
            ).Then(id => {
                Assert.IsNull(id);
            }).AsCoroutine();
        }
    }
}