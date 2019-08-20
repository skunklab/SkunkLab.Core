using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Piraeus.Extensions.Configuration;
using Piraeus.Configuration;
using Piraeus.Extensions.Gateways;
using Piraeus.Extensions.Options;

namespace Piraeus.TcpGateway
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            OrleansConfig oconfig = null;
            PiraeusConfig pconfig = null;

            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddOrleansConfiguration(out oconfig);
            configBuilder.AddPiraeusConfiguration(out pconfig);

            PiraeusGatewayOptions pgo = new PiraeusGatewayOptions(oconfig);
            
            IServiceProvider sp = services.BuildServiceProvider();
            LoggerType loggerType = oconfig.GetLoggerTypes();
            if(!loggerType.HasFlag(LoggerType.None))
            {
                services.AddLogging(builder =>
                {
                    if (loggerType.HasFlag(LoggerType.Console))
                        builder.AddConsole();
                    if (loggerType.HasFlag(LoggerType.Debug))
                        builder.AddDebug();
                    if (loggerType.HasFlag(LoggerType.AppInsights) && !string.IsNullOrEmpty(oconfig.AppInsightsKey))
                        builder.AddApplicationInsights(oconfig.AppInsightsKey);
                       
                    builder.SetMinimumLevel(LogLevel.Warning);
                });
            }
            
            TcpGatewayService tgs = sp.GetRequiredService<TcpGatewayService>();
            tgs.Init(oconfig.Dockerized);


            
        }
    }
}
