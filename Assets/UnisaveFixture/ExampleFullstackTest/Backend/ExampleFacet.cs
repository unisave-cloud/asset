using Unisave.Facets;

namespace UnisaveFixture.ExampleFullstackTest.Backend
{
    public class ExampleFacet : Facet
    {
        public string Echo(string message)
        {
            return message;
        }
    }
}