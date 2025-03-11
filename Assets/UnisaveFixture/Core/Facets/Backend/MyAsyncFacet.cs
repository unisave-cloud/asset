using System;
using System.Threading.Tasks;
using LightJson;
using Unisave.Facets;

namespace UnisaveFixture.Core.Facets.Backend
{
    /// <summary>
    /// Analogous to MyFacet, only having all methods asynchronous
    /// </summary>
    public class MyAsyncFacet : Facet
    {
        public async Task ReturnsTask()
        {
            // dummy await something
            await Task.Delay(1); // 1ms
            
            // do nothing
        }
        
        public async Task<string> EchoesStringAsync(string given)
        {
            // dummy await something
            await Task.Delay(1); // 1ms
            
            return given;
        }
        
        public async Task<JsonValue> EchoesJsonValueAsync(JsonValue given)
        {
            // dummy await something
            await Task.Delay(1); // 1ms
            
            return given;
        }
        
        public async Task ThrowsExceptionWithMessageAsync(string message)
        {
            // dummy await something
            await Task.Delay(1); // 1ms
            
            throw new Exception(message);
        }

        // ReSharper disable once AsyncVoidMethod
        public async void AsyncVoidCannotBeCalled()
        {
            // dummy await something
            await Task.Delay(1); // 1ms
            
            throw new Exception("Method was called, but it shouldn't!");
        }
    }
}