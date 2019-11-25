using Microsoft.AspNetCore.Builder;

namespace Piraeus.WebSocketGateway.Middleware
{
    public static class PiraeusWebSocketMiddlewareExtensions
    {
        public static IApplicationBuilder UsePiraeusWS(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PiraeusWebSocketMiddleware>();

        }


    }
}
