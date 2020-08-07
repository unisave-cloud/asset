using System;
using Unisave;
using Unisave.Facets;
using Unisave.Foundation;

namespace UnisaveFixture.Backend.Core.Facets
{
    [Middleware(typeof(MyMiddleware), "C1")]
    [Middleware(2, typeof(MyMiddleware), "C2")]
    public class FacetWithMiddleware : Facet
    {
        /// <summary>
        /// This field is edited by the middleware as it runs
        /// </summary>
        public static string middlewareLog;
        
        [Middleware(-5, typeof(MyMiddleware), "M1")]
        [Middleware(5, typeof(MyMiddleware), "M2")]
        public void MethodWithMiddleware()
        {
            MyMiddleware.AppendToLog("B1");
        }

        public void MethodWithoutMiddleware()
        {
            MyMiddleware.AppendToLog("B2");
        }
    }
    
    public class MyMiddleware : FacetMiddleware
    {
        public MyMiddleware(Application app) : base(app) { }
        
        public override FacetResponse Handle(
            FacetRequest request,
            Func<FacetRequest, FacetResponse> next,
            string[] parameters
        )
        {
            AppendToLog(parameters[0]);
            
            var response = next.Invoke(request);

            AppendToLog(parameters[0] + "'");

            return response;
        }
        
        public static void AppendToLog(string item)
        {
            string log = FacetWithMiddleware.middlewareLog;

            if (log == null)
                log = "";

            if (log.Length != 0)
                log += ",";

            log += item;
            
            FacetWithMiddleware.middlewareLog = log;
        }
    }
}