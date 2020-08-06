using Unisave.Facets;

namespace UnisaveFixture.Backend.ExampleTesting
{
    public class ExampleTestingFacet : Facet
    {
        public int AddNumbers(int a, int b)
        {
            return a + b;
        }
    }
}