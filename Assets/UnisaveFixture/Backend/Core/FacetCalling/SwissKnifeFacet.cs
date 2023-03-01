using System;
using Unisave.Facets;

namespace UnisaveFixture.Backend.Core.FacetCalling
{
    public class SwissKnifeFacet : Facet
    {
        public void EmptyMethod()
        {
            // does nothing
        }
        
        public string Ping(string message)
        {
            return "Pong: " + message;
        }

        public void ThrowInvalidOperationException()
        {
            throw new InvalidOperationException();
        }
    }
}