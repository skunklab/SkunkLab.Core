using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Piraeus.Configuration;
using Piraeus.Core.Logging;
using Piraeus.Extensions.Configuration;
using Piraeus.Extensions.Logging;

namespace Piraeus.UdpGateway
{
    class Program
    {
        

        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //PiraeusConfig config = null;
                    services.AddPiraeusConfiguration(out PiraeusConfig config);
                    if (!string.IsNullOrEmpty(config.AppInsightsKey))
                    {
                        services.AddApplicationInsightsTelemetry(op =>
                        {
                            op.InstrumentationKey = config.AppInsightsKey;
                            op.AddAutoCollectedMetricExtractor = true;
                            op.EnableHeartbeat = true;
                        });
                    }

                    services.AddOrleansConfiguration(); //add orleans config as singleton
                    services.AddLogging(builder => builder.AddLogging(config));
                    services.AddSingleton<Logger>();    //add the logger
                    services.AddHostedService<UdpGatewayHost>(); //start the service
                });


    }
}
