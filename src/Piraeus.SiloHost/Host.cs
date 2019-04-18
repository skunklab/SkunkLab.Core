using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Clustering.Redis;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Storage.Redis;
using Piraeus.Configuration;
using Piraeus.Grains;
using System;
using System.Net;

namespace Piraeus.SiloHost
{
    public class Host
    {
        private OrleansConfig orleansConfig;
        private ISiloHost host;

        public Host()
        {
        }

        public void Init()
        {
            orleansConfig = GetOrleansConfiguration();

            if(orleansConfig.Dockerized)
            {
                CreateClusteredSiloHost();                
            }
            else
            {
                CreateLocalSiloHost();
            }

            host.StartAsync().GetAwaiter();

        }

        private void CreateLocalSiloHost()
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

            host = builder.Build();
        }

        private void CreateClusteredSiloHost()
        {
            var silo = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = orleansConfig.ClusterId;
                    options.ServiceId = orleansConfig.ServiceId;
                });
                //.EnableDirectClient();

            string storageType = GetStorageType(orleansConfig.DataConnectionString);
            if (storageType.ToLowerInvariant() == "redis")
            {
                silo.UseRedisMembership(options => options.ConnectionString = orleansConfig.DataConnectionString);
                silo.AddRedisGrainStorage("store", options => options.ConnectionString = orleansConfig.DataConnectionString);
            }
            else if (storageType.ToLowerInvariant() == "azurestorage")
            {
                silo.UseAzureStorageClustering(options => options.ConnectionString = orleansConfig.DataConnectionString);
                silo.AddAzureBlobGrainStorage("store", options => options.ConnectionString = orleansConfig.DataConnectionString);
            }
            else
            {
                //throw
            }

            silo.ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000);

            LogLevel orleansLogLevel = GetLogLevel();
            var loggers = orleansConfig.GetLoggerTypes();
            silo.ConfigureLogging(builder =>
            {                
                if (loggers.HasFlag(LoggerType.Console))
                    builder.AddConsole();
                if (loggers.HasFlag(LoggerType.Debug))
                    builder.AddDebug();
                builder.SetMinimumLevel(orleansLogLevel);
            });

            if (loggers.HasFlag(LoggerType.AppInsights) && !string.IsNullOrEmpty(orleansConfig.AppInsightsKey))
                silo.AddApplicationInsightsTelemetryConsumer(orleansConfig.AppInsightsKey);
            host = silo.Build();            

            var clusterClient = (IClusterClient)host.Services.GetService(typeof(IClusterClient));
            GraphManager.Initialize(clusterClient);
        }
                
        private LogLevel GetLogLevel()
        {
            return Enum.Parse<LogLevel>(orleansConfig.LogLevel, true);
        }

        private string GetStorageType(string connectionString)
        {
            string cs = connectionString.ToLowerInvariant();
            if (cs.Contains(":6380") || cs.Contains(":6379"))
            {
                return "Redis";
            }
            else if (cs.Contains("defaultendpointsprotocol="))
            {
                return "AzureStorage";
            }
            else
            {
                throw new ArgumentException("Invalid connection string");
            }

        }

        private OrleansConfig GetOrleansConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("./orleansconfig.json") 
                .AddEnvironmentVariables("OR_");
            IConfigurationRoot root = builder.Build();
            OrleansConfig config = new OrleansConfig();
            root.Bind(config);
            return config;
        }
    }
}
