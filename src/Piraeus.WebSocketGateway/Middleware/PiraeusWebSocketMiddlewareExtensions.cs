using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SkunkLab.Channels.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Piraeus.WebSocketGateway.Middleware
{
    public static class PiraeusWebSocketMiddlewareExtensions
    {
        public static IApplicationBuilder UsePiraeusWS(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PiraeusWebSocketMiddleware>();

        }

       

        //public static IServiceCollection AddPiraeusWebSocket(this IServiceCollection services)
        //{
        //    services.AddTransient<PiraeusWebSocketMiddleware>();

        //    return services;
        //}
    }
}
