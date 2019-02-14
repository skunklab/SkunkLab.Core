using Microsoft.AspNetCore.Builder;

namespace Piraeus.WebGateway.Middleware
{
    public static class WebSocketMiddlewareExtensions
    {
        public static IApplicationBuilder UseSOAPEndpoint(this IApplicationBuilder builder)
        {

            return builder.UseMiddleware<WebSocketMiddleware>();

        }
    }
}
