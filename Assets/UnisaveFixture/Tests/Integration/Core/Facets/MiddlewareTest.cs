using NUnit.Framework;
using Unisave.Facades;
using Unisave.Testing;
using UnisaveFixture.Backend.Core.Facets;

namespace UnisaveFixture.Tests.Core.Facets
{
    public class MiddlewareTest : BackendTestCase
    {
        [Test]
        public void ItRunsMiddlewareAsProperlyDefined()
        {
            FacetWithMiddleware.middlewareLog = null;
            
            OnFacet<FacetWithMiddleware>.CallSync(
                nameof(FacetWithMiddleware.MethodWithoutMiddleware)
            );
            Assert.AreEqual(
                "C1,C2,B2,C2',C1'",
                FacetWithMiddleware.middlewareLog
            );
            
            FacetWithMiddleware.middlewareLog = null;
            OnFacet<FacetWithMiddleware>.CallSync(
                nameof(FacetWithMiddleware.MethodWithMiddleware)
            );
            Assert.AreEqual(
                "C1,C2,M1,M2,B1,M2',M1',C2',C1'",
                FacetWithMiddleware.middlewareLog
            );
        }
    }
}