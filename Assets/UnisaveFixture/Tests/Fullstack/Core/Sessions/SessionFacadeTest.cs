using System.Collections;
using LightJson;
using NUnit.Framework;
using Unisave.Facades;
using UnisaveFixture.Backend.Core;
using UnisaveFixture.Backend.Core.Sessions;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace UnisaveFixture.Tests.Core.Sessions
{
    public class SessionFacadeTest
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return OnFacet<FullstackUtilsFacet>.Call(
                nameof(FullstackUtilsFacet.ClearDatabase)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator ItRemembersValuesBetweenRequests()
        {
            yield return OnFacet<SessionFacet>.Call<JsonObject>(
                nameof(SessionFacet.GetSessionRecord)
            ).Then(r => {
                Assert.IsNull(r);
            }).AsCoroutine();
            
            yield return OnFacet<SessionFacet>.Call<Vector3>(
                nameof(SessionFacet.Get),
                "foo",
                Vector3.zero
            ).Then(v => {
                Assert.AreEqual(Vector3.zero, v);
            }).AsCoroutine();
            
            yield return OnFacet<SessionFacet>.Call(
                nameof(SessionFacet.Set),
                "foo",
                Vector3.up
            ).AsCoroutine();
            
            yield return OnFacet<SessionFacet>.Call<Vector3>(
                nameof(SessionFacet.Get),
                "foo",
                Vector3.zero
            ).Then(v => {
                Assert.AreEqual(Vector3.up, v);
            }).AsCoroutine();
            
            yield return OnFacet<SessionFacet>.Call<JsonObject>(
                nameof(SessionFacet.GetSessionRecord)
            ).Then(r => {
                Assert.IsNotNull(r);
            }).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator ItUpdatesSessionExpirationWithEveryRequest()
        {
            yield return OnFacet<SessionFacet>.Call(
                nameof(SessionFacet.Set),
                "foo",
                Vector3.up
            ).AsCoroutine();

            string oldSessionExpiration = null;
            
            yield return OnFacet<SessionFacet>.Call<JsonObject>(
                nameof(SessionFacet.GetSessionRecord)
            ).Then(r => {
                oldSessionExpiration = r["expiresAt"].AsString;
            }).AsCoroutine();
            
            yield return OnFacet<SessionFacet>.Call(
                nameof(SessionFacet.EmptyFacetMethod)
            ).AsCoroutine();
            
            yield return OnFacet<SessionFacet>.Call<JsonObject>(
                nameof(SessionFacet.GetSessionRecord)
            ).Then(r => {
                Assert.AreNotEqual(
                    oldSessionExpiration,
                    r["expiresAt"].AsString
                );
            }).AsCoroutine();
        }
    }
}