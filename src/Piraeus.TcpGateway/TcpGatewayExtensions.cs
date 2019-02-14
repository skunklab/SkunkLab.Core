using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Clustering.Redis;
using Orleans.Hosting;
using Orleans.Runtime;
using Piraeus.Configuration.Core;
using Piraeus.Configuration.Settings;
using Piraeus.Extensions.Logging;
using Piraeus.GrainInterfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using testME = Orleans.Configuration;


namespace Piraeus.TcpGateway
{
    public static class TcpGatewayExtensions
    {
        private static int attempt;
        private static readonly int initializeAttemptsBeforeFailing = 8;

        
        public static IServiceCollection AddOrleansConfiguration(this IServiceCollection services)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Environment.CurrentDirectory + "\\orleansconfig.json")
                .AddEnvironmentVariables("OR_");
            IConfigurationRoot root = builder.Build();
            OrleansConfig config = new OrleansConfig();
            ConfigurationBinder.Bind(root, config);
            services.AddSingleton<OrleansConfig>(config);

            return services;
        }

        public static IServiceCollection AddPiraeusConfiguration(this IServiceCollection services)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Environment.CurrentDirectory + "\\piraeusconfig.json")
                .AddEnvironmentVariables("PI_");
            IConfigurationRoot root = builder.Build();
            PiraeusConfig config = new PiraeusConfig();
            ConfigurationBinder.Bind(root, config);
            services.AddSingleton<PiraeusConfig>(config);

            return services;
        }

        public static IServiceCollection AddOrleansClusterClient(this IServiceCollection services, ILoggerFactory loggerFactory,
            Action<TcpGatewayOptions> configureOptions)
        {
            return services.AddOrleansClusterClient(loggerFactory, ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection AddOrleansClusterClient(this IServiceCollection services, ILoggerFactory loggerFactory,
            Action<Microsoft.Extensions.Options.OptionsBuilder<TcpGatewayOptions>> configureOptions)
        {
            configureOptions?.Invoke(services.AddOptions<TcpGatewayOptions>());
            services.AddSingleton<ILoggerFactory>(CreateLoggerFactory);
            services.AddSingleton<IClusterClient>(CreateClusterClient);
            return services.AddSingleton<TcpGatewayService>();
        }

        private static bool HasLoggerType(OrleansConfig config, string typeName)
        {
            string[] loggerTypes = config.LoggerTypes?.Split(";", StringSplitOptions.RemoveEmptyEntries);
            return !string.IsNullOrEmpty(loggerTypes.Where((t) => t.ToLowerInvariant() == typeName).FirstOrDefault());
        }

        private static LogLevel GetLogLevel(OrleansConfig config)
        {
            return Enum.Parse<LogLevel>(config.LogLevel, true);
        }


        private static ILoggerFactory CreateLoggerFactory(IServiceProvider serviceProvider)
        {
            
            OrleansConfig config = serviceProvider.GetService<OrleansConfig>();
            ILoggerFactory loggerFactory = new LoggerFactory();
            LogLevel logLevel = Enum.Parse<LogLevel>(config.LogLevel, true);
            string[] loggerTypes = config.LoggerTypes?.Split(";", StringSplitOptions.RemoveEmptyEntries);
            
            if(HasLoggerType(config, "console"))
            {
                loggerFactory.AddConsole(logLevel);
            }

            if (HasLoggerType(config, "debug"))
            {
                loggerFactory.AddDebug(logLevel);
            }

            if(HasLoggerType(config, "appinsights"))
            {
                AppInsightsOptions appOptions = new AppInsightsOptions()
                {
                    DeveloperMode = false,
                    InstrumentationKey = config.AppInsightsKey
                    
                };

                loggerFactory.AddAppInsights(appOptions);
            }        

            return loggerFactory;            
        }

        private static IClusterClient CreateClusterClient(IServiceProvider serviceProvider)
        {
            OrleansConfig config = serviceProvider.GetService<OrleansConfig>();
            string storageType = GetStorageType(config.DataConnectionString);

            TcpGatewayOptions options = serviceProvider.GetOptionsByName<TcpGatewayOptions>("TcpGatewayOptions");

            if (config.Dockerized)
            {
                var localClient = new ClientBuilder()
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
                .UseLocalhostClustering()
                .Build();

                localClient.Connect(RetryFilter).GetAwaiter();
                return localClient;
            }
            else
            {
                var client = new ClientBuilder()
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
                    .Configure<testME.ClusterOptions>(op =>
                    {
                        op.ClusterId = config.ClusterId;
                        op.ServiceId = config.ServiceId;
                    });

              

                if(HasLoggerType(config, "appinsights"))
                {                    
                    client.AddApplicationInsightsTelemetryConsumer(config.AppInsightsKey);
                }
               
                if (storageType == "Redis")
                {
                    ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                    ILogger<RedisGatewayListProvider> logger = loggerFactory.CreateLogger<RedisGatewayListProvider>();
                    client.UseRedisGatewayListProvider(logger, op =>
                            op.ConnectionString = config.DataConnectionString
                        );                    
                }
                else if(storageType == "AzureStorage")
                {
                    client.UseAzureStorageClustering(op => op.ConnectionString = config.DataConnectionString);
                }

                IClusterClient clusterClient = client.Build();

                clusterClient.Connect(RetryFilter).GetAwaiter();
                return clusterClient;
            }
        }

        private static string GetStorageType(string connectionString)
        {
            string cs = connectionString.ToLowerInvariant();
            if(cs.Contains(":6380") || cs.Contains(":6379"))
            {
                return "Redis";
            }
            else if (cs.Contains("defaultendpointsprotocol=") && cs.Contains("accountname=") && cs.Contains("accountkey="))
            {
                return "AzureStorage";
            }
            else
            {
                throw new ArgumentException("Invalid connection string");
            }

        }


        private static async Task<bool> RetryFilter(Exception exception)
        {
            if (exception.GetType() != typeof(SiloUnavailableException))
            {
                Console.WriteLine($"Cluster client failed to connect to cluster with unexpected error.  Exception: {exception}");
                return false;
            }
            attempt++;
            Console.WriteLine($"Cluster client attempt {attempt} of {initializeAttemptsBeforeFailing} failed to connect to cluster.  Exception: {exception}");
            if (attempt > initializeAttemptsBeforeFailing)
            {
                return false;
            }
            await Task.Delay(TimeSpan.FromSeconds(10));
            return true;
        }




    }
}
