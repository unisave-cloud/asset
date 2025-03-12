using System;
using System.Threading.Tasks;
using Unisave.Facades;
using Unisave.Facets;

namespace UnisaveFixture.Core.HttpClient.Backend
{
    public class HttpClientFacet : Facet
    {
        public void GetWebsiteSync()
        {
            var response = Http.Get("https://unisave.cloud/");
            
            response.Throw();
        }

        public async Task GetWebsiteAsync()
        {
            var response = await Http.GetAsync("https://unisave.cloud/");
            
            response.Throw();
        }

        public async Task<bool> TimeoutRequest()
        {
            try
            {
                await Http.WithTimeout(
                    TimeSpan.FromMilliseconds(0.1)
                ).GetAsync("https://unisave.cloud/");
            }
            catch (TaskCanceledException e)
            {
                Log.Info("Request timed out: " + e);
                
                return true;
            }
            
            return false;
        }
    }
}