using System.Collections;
using System.Collections.Generic;
using LightJson;
using NUnit.Framework;
using Unisave.Facades;
using Unisave.Serialization;
using UnisaveFixture.Backend.Core.Arango;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Core.Arango
{
    public class RawAqlTest
    {
        [UnityTest]
        public IEnumerator ItCanReturnResults()
        {
            yield return OnFacet<RawAqlFacet>.Call<List<JsonValue>>(
                nameof(RawAqlFacet.Get),
                @"
                    FOR i IN 1..4
                        RETURN i
                ",
                null
            ).Then(results => {
                Assert.AreEqual(
                    "[1,2,3,4]".Replace('\'', '"'),
                    Serializer.ToJson(results).ToString()
                );
            }).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator ItCanPassBindVarsResults()
        {
            yield return OnFacet<RawAqlFacet>.Call<List<JsonValue>>(
                nameof(RawAqlFacet.Get),
                @"
                    RETURN @foo
                ",
                new JsonObject {
                    ["foo"] = 24
                }
            ).Then(results => {
                Assert.AreEqual(
                    "[24]".Replace('\'', '"'),
                    Serializer.ToJson(results).ToString()
                );
            }).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator ItCanDeserializeResults()
        {
            yield return OnFacet<RawAqlFacet>.Call<Vector3>(
                nameof(RawAqlFacet.FirstAsVector),
                @"
                    RETURN { x: 1, y: 2, z: 3 }
                "
            ).Then(v => {
                Assert.AreEqual(1, v.x);
                Assert.AreEqual(2, v.y);
                Assert.AreEqual(3, v.z);
            }).AsCoroutine();
        }
    }
}