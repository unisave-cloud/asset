using System.Reflection;
using System.Threading.Tasks;

namespace Unisave.Facets
{
    /// <summary>
    /// Defines the facet calling API at the application-level
    /// </summary>
    public interface IApplicationLayerFacetCaller
    {
        Task<object> CallFacetMethodAsync(
            MethodInfo method,
            object[] arguments
        );
    }
}