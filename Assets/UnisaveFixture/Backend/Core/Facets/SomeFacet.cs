using System;
using Unisave.Facets;

namespace UnisaveFixture.Backend.Core.Facets
{
    /// <summary>
    /// Some facet that we run tests on
    /// </summary>
    public class SomeFacet : Facet
    {
        public static bool flag;

        public void MyProcedure()
        {
            flag = true;
        }

        // static methods should not be included in the lookup
        public static void MyProcedure(int foo) {}

        public void MyParametrizedProcedure(bool setFlagTo)
        {
            flag = setFlagTo;
        }

        public void AmbiguousMethod() {}
        private void AmbiguousMethod(int foo) {}

        private void PrivateProcedure() {}

        public int SquaringFunction(int x)
        {
            return x * x;
        }

        public void ExceptionalMethod()
        {
            throw new Exception("Some interesting exception.");
        }
    }
}