using System;
using LightJson;
using Unisave.Facets;

namespace UnisaveFixture.Core.Facets.Backend
{
    /// <summary>
    /// A dummy facet designed to test the facet invocation logic itself.
    /// The goal is to list all possible ways a facet can be called and
    /// test that it does get called properly.
    /// </summary>
    public class MyFacet : Facet
    {
        public void ReturnsVoid()
        {
            // do nothing
        }

        public string EchoesString(string given)
        {
            return given;
        }
        
        public int EchoesInt(int given)
        {
            return given;
        }
        
        public double EchoesDouble(double given)
        {
            return given;
        }
        
        public JsonValue EchoesJsonValue(JsonValue given)
        {
            return given;
        }
    
        public void ThrowsExceptionWithMessage(string message)
        {
            throw new Exception(message);
        }
    }
}