using dynapi.poc.apigen.Entities;
using dynapi.poc.apigen.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace dynapi.poc.apigen.Middlewares
{
    /// <summary>
    /// Error handler and formatter, 
    /// if founds an exception tryies to parse it to a representative state
    /// </summary>
    public class ErrorFormatterMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;

        #endregion

        #region Constructor

        public ErrorFormatterMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Worker

        public Task Invoke(HttpContext context)
        {
            try
            {
                return _next.Invoke(context);
            }
            catch (Exception e)
            {
                return HandleError(context, e);
            }
        }

        #endregion

        #region Private methods

        private async Task HandleError(HttpContext c, Exception e)
        {
            c.Response.ContentType = "application/problem+json";

            if (e is DomainException dx)
            {
                c.Response.StatusCode = dx.StatusCode;
                await c.Response.WriteAsync(JsonConvert.SerializeObject(
                    new ErrorResponse(e.GetType().Name, e.Message)));
            }
            else if (e is IncorrectVerbException ix)
            {
                c.Response.StatusCode = ix.StatusCode;
                await c.Response.WriteAsync(JsonConvert.SerializeObject(
                    new ErrorResponse(e.GetType().Name, e.Message)));
            }
            else
            {
                c.Response.StatusCode = 500;
                await c.Response.WriteAsync(JsonConvert.SerializeObject(
                    new ErrorResponse("InternalError", "Sever internal error")));
            }

            #endregion
        }
    }
}
