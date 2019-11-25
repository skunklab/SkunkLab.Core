using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Clustering.Redis;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Storage.Redis;
using Piraeus.Configuration;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.SiloHost
{
    public class SiloHostService : IHostedService
    {
        public SiloHostService(OrleansConfig orleansConfig)
        {
            this.orleansConfig = orleansConfig;
        }

        private readonly OrleansConfig orleansConfig;
        private ISiloHost host;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
#if DEBUG
            host = AddLocalSiloHost();
#else
            host = AddClusteredSiloHost();
#endif
            //if (orleansConfig.Dockerized)
            //{
            //    host = AddClusteredSiloHost();
            //}
            //else
            //{
            //    host = AddLocalSiloHost();
            //}

            await host.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (host != null)
            {
                await host.StopAsync(cancellationToken);
            }
        }

        private ISiloHost AddLocalSiloHost()
        {
            var builder = new SiloHostBuilder()
            // Use localhost clustering for a single local silo
            .UseLocalhostClustering()
            // Configure ClusterId and ServiceId
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = orleansConfig.ClusterId;
                options.ServiceId = orleansConfig.ServiceId;
            })
            .AddMemoryGrainStorage("store")
            // Configure connectivity
            .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)

            // Configure logging with any logging framework that supports Microsoft.Extensions.Logging.
            // In this particular case it logs using the Microsoft.Extensions.Logging.Console package.
            .ConfigureLogging(logging => logging.AddConsole());

            return builder.Build();
        }

        private ISiloHost AddClusteredSiloHost()
        {
            var silo = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = orleansConfig.ClusterId;
                    options.ServiceId = orleansConfig.ServiceId;
                });


            if (string.IsNullOrEmpty(orleansConfig.DataConnectionString))
            {
                silo.AddMemoryGrainStorage("store");
            }
            else if (orleansConfig.DataConnectionString.Contains("6379") ||
                orleansConfig.DataConnectionString.Contains("6380"))
            {
                silo.UseRedisMembership(options => options.ConnectionString = orleansConfig.DataConnectionString);
                silo.AddRedisGrainStorage("store", options => options.ConnectionString = orleansConfig.DataConnectionString);
            }
            else
            {
                silo.UseAzureStorageClustering(options => options.ConnectionString = orleansConfig.DataConnectionString);
                silo.AddAzureBlobGrainStorage("store", options => options.ConnectionString = orleansConfig.DataConnectionString);
            }

            silo.ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000);

            LogLevel orleansLogLevel = Enum.Parse<LogLevel>(orleansConfig.LogLevel);
            var loggers = orleansConfig.GetLoggerTypes();
            silo.ConfigureLogging(builder =>
            {
                if (loggers.HasFlag(LoggerType.Console))
                    builder.AddConsole();
                if (loggers.HasFlag(LoggerType.Debug))
                    builder.AddDebug();
                builder.SetMinimumLevel(orleansLogLevel);
            });

            if (!string.IsNullOrEmpty(orleansConfig.AppInsightsKey))
                silo.AddApplicationInsightsTelemetryConsumer(orleansConfig.AppInsightsKey);

            return silo.Build();

        }
    }
}
