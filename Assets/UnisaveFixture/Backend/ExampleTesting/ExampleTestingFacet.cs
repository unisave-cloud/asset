using Unisave.Facets;

namespace UnisaveFixture.Backend.ExampleTesting
{
    public class ExampleTestingFacet : Facet
    {
        // flag for asserting
        public static bool addNumbersWasCalled = false;
        
        public int AddNumbers(int a, int b)
        {
            addNumbersWasCalled = true;
            return a + b;
        }
    }
}