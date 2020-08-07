namespace UnisaveFixture.Backend.Core.Facets
{
    /// <summary>
    /// This is a facet with MyProcedure that does not inherit
    /// from the Facet class and so will not be called
    /// </summary>
    public class WrongFacet // no parent
    {
        public void MyProcedure() {}
    }
}