using dynapi.poc.apigen.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dynapi.poc.apigen.Endpoints
{
    /// <summary>
    /// i've done this just to delegate some behavior
    /// </summary>
    public class EndpointCollection : List<Endpoint>
    {
        public IEndpointHandler Find(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var posibleEndpoints = this.Where(e => context.Request.Path.Equals(e.Path, StringComparison.OrdinalIgnoreCase));

            if(posibleEndpoints.Any())
            {
                var endpoint = posibleEndpoints
                    .FirstOrDefault(e => context.Request.Method.Equals(e.Verb, StringComparison.OrdinalIgnoreCase));

                if(endpoint != null)
                {
                    return GetEndpointHandler(endpoint, context);
                }

                throw new IncorrectVerbException(context.Request.Method, context.Request.Path);
            }

            return null;
        }

        private IEndpointHandler GetEndpointHandler(Endpoint endpoint, HttpContext context) => 
            context.RequestServices.GetService(endpoint.Handler) as IEndpointHandler;
    }
}
