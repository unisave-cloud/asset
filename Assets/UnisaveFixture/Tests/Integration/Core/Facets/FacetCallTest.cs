using NUnit.Framework;
using Unisave.Facades;
using Unisave.Facets;
using Unisave.Testing;
using Unisave.Utils;
using UnisaveFixture.Backend.Core.Facets;

namespace UnisaveFixture.Tests.Core.Facets
{
    public class FacetCallTest : BackendTestCase
    {
        [Test]
        public void ItRunsMyProcedure()
        {
            SomeFacet.flag = false;
            
            Assert.IsNull(SessionId);
            
            OnFacet<SomeFacet>.CallSync(
                nameof(SomeFacet.MyProcedure)
            );
            
            Assert.IsNotNull(SessionId);

            Assert.IsTrue(SomeFacet.flag);
        }

        [Test]
        public void ItRunsMyParametrizedProcedure()
        {
            SomeFacet.flag = false;
            
            OnFacet<SomeFacet>.CallSync(
                nameof(SomeFacet.MyParametrizedProcedure),
                true
            );
            Assert.IsTrue(SomeFacet.flag);
            
            OnFacet<SomeFacet>.CallSync(
                nameof(SomeFacet.MyParametrizedProcedure),
                false
            );
            Assert.IsFalse(SomeFacet.flag);
        }

        [Ignore("Test does not work, should await the async operation.")]
        [Test]
        public void ItChecksParentFacet()
        {
            var e = Assert.Catch<FacetSearchException>(() => {
                ClientApp.Services.Resolve<FacetCaller>().CallFacetMethod(
                    typeof(WrongFacet),
                    nameof(SomeFacet.MyProcedure)
                ).Done();
            });
            
            StringAssert.Contains("Facet 'WrongFacet' was not found.", e.Message);
        }

        [Test]
        public void ItChecksMethodExistence()
        {
            var e = Assert.Catch<MethodSearchException>(() => {
                OnFacet<SomeFacet>.CallSync(
                    "NonExistingMethod"
                );
            });
            
            StringAssert.Contains("doesn't have public method called", e.ToString());
        }

        [Test]
        public void ItChecksAmbiguousMethods()
        {
            var e = Assert.Catch<MethodSearchException>(() => {
                OnFacet<SomeFacet>.CallSync(
                    nameof(SomeFacet.AmbiguousMethod),
                    false
                );
            });
            
            StringAssert.Contains("has multiple methods called", e.ToString());
        }

        [Test]
        public void ItChecksPublicMethods()
        {
            var e = Assert.Catch<MethodSearchException>(() => {
                OnFacet<SomeFacet>.CallSync(
                    "PrivateProcedure"
                );
            });
            
            StringAssert.Contains("has to be public in order to be called", e.ToString());
        }

        [Test]
        public void ItRunsFunctions()
        {
            int result = OnFacet<SomeFacet>.CallSync<int>(
                nameof(SomeFacet.SquaringFunction),
                5
            );
            
            Assert.AreEqual(25, result);
        }
    }
}