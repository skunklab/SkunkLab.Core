using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Piraeus.Configuration;
using Piraeus.Core.Logging;
using Piraeus.Extensions.Configuration;
using Piraeus.Extensions.Logging;
using Piraeus.Extensions.Orleans;
using Piraeus.WebSocketGateway.Middleware;
using System;

namespace Piraeus.WebSocketGateway
{
    public class Startup
    {


        public void Configure(IApplicationBuilder app)
        {

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });


            app.UseAuthentication();
            app.UseWebSockets();
            app.UseMiddleware<PiraeusWebSocketMiddleware>();

            //app.UseMvc();

        }

        public void ConfigureServices(IServiceCollection services)
        {
            //PiraeusConfig config = null;
            //OrleansConfig orleansConfig = null;
            services.AddPiraeusConfiguration(out PiraeusConfig config);
            services.AddOrleansConfiguration(out OrleansConfig orleansConfig);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransientOrleansClusterClient(orleansConfig);
            LoggerType loggers = config.GetLoggerTypes();

            if (loggers.HasFlag(Piraeus.Configuration.LoggerType.AppInsights))
            {
                services.AddApplicationInsightsTelemetry(op =>
                {
                    op.InstrumentationKey = config.AppInsightsKey;
                    op.AddAutoCollectedMetricExtractor = true;
                    op.EnableHeartbeat = true;
                });
            }
            services.AddLogging(builder => builder.AddLogging(config));
            services.AddSingleton<Logger>();            

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = !string.IsNullOrEmpty(config.ClientIssuer),
                        ValidateAudience = !string.IsNullOrEmpty(config.ClientAudience),
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config.ClientIssuer,
                        ValidAudience = config.ClientAudience,
                        ClockSkew = TimeSpan.FromMinutes(5.0),
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(config.ClientSymmetricKey))
                    };
                });

            services.AddMvc(option => option.EnableEndpointRouting = true);

            services.AddRouting();
            services.AddMvcCore();
        }


    }
}
