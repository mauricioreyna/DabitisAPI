using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace dynapi.poc.apigen.Endpoints
{
    /// <summary>
    /// An interface to define how an endpoint handler should be designed
    /// </summary>
    public interface IEndpointHandler
    {
        /// <summary>
        /// Worker process, i should remove object parametrization
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<object> ProcessAsync(HttpContext context);
    }
}
