using System.Collections;
using AcceptanceTests.Backend.SingleEntityOperations;
using NUnit.Framework;
using Unisave.Facades;
using UnityEngine;
using UnityEngine.TestTools;

namespace AcceptanceTests
{
    public class SingleEntityOperationsTest
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return OnFacet<SeoFacet>.Call(
                nameof(SeoFacet.SetUp)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator NonExistingEntityCannotBeFound()
        {
            yield return OnFacet<SeoFacet>.Call(
                nameof(SeoFacet.NonExistingEntityCannotBeFound)
            ).AsCoroutine();
        }

        [UnityTest]
        public IEnumerator EntityCanBeCreatedQueriedAndDeleted()
        {
            yield return OnFacet<SeoFacet>.Call(
                nameof(SeoFacet.EntityCanBeCreatedQueriedAndDeleted)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator EntityCanBeFoundById()
        {
            yield return OnFacet<SeoFacet>.Call(
                nameof(SeoFacet.EntityCanBeFoundById)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator EntityCanBeFoundByStringAttribute()
        {
            yield return OnFacet<SeoFacet>.Call(
                nameof(SeoFacet.EntityCanBeFoundByStringAttribute)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator EntityCanBeFoundByEnumAttribute()
        {
            yield return OnFacet<SeoFacet>.Call(
                nameof(SeoFacet.EntityCanBeFoundByEnumAttribute)
            ).AsCoroutine();
        }
    }
}