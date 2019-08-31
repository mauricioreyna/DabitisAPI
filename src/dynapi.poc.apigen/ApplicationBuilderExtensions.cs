using dynapi.poc.apigen.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiGen(this IApplicationBuilder app)
        {
            return app
                .UseMiddleware<ErrorFormatterMiddleware>()
                .UseMiddleware<ApiGenMiddleware>();
        }
    }
}
