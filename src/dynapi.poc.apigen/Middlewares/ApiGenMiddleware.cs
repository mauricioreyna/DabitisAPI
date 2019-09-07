using dynapi.poc.apigen.Endpoints;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace dynapi.poc.apigen.Middlewares
{
    /// <summary>
    /// This middleware tries to find a defined endpoint and executes him
    /// If nothing is found, next in pipeline is called
    /// </summary>
    public class ApiGenMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;

        #endregion

        #region Constructors
        public ApiGenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Worker Methods

        public Task Invoke(HttpContext context, EndpointCollection endpoints)
        {
            var endpoint = endpoints.Find(context);

            if (endpoint != null)
            {
                return endpoint.ProcessAsync(context);
            }

            return _next.Invoke(context);
        }

        #endregion
    }
}
