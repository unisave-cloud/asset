using System;
using Unisave.Facets;

namespace UnisaveFixture.Backend.Core.FacetCalling
{
    public class FcFacet : Facet
    {
        public void VoidFacet()
        {
            // do nothing
        }
    
        public void VoidFacetThatThrows(string message)
        {
            throw new Exception(message);
        }
    }
}
