//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Orleans;
//using Piraeus.Configuration.Core;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Linq;
//using Piraeus.Extensions.Logging;
//using Piraeus.GrainInterfaces;
//using Orleans.Clustering.Redis;
//using System.Threading.Tasks;
//using Orleans.Runtime;
//using LoggerType = Piraeus.Configuration.Core.LoggerType;

//namespace Piraeus.Extensions.Orleans
//{
//    public static class OrleansExtensions
//    {
//        private static int attempt;
//        private static readonly int initializeAttemptsBeforeFailing = 8;

//        public static IServiceCollection AddPiraeusLoggers(this ServiceCollection services, OrleansConfig config)
//        {
//            LoggerType loggerType = config.GetLoggerTypes();

//            services.AddLogging(builder =>
//            {
//                if (loggerType.HasFlag(LoggerType.Console))
//                    builder.AddConsole();

//                if (loggerType.HasFlag(LoggerType.Debug))
//                    builder.AddDebug();

//                builder.SetMinimumLevel(Enum.Parse<LogLevel>(config.OrleansLogLevel));
//            });

//            if (loggerType.HasFlag(LoggerType.AppInsights))
//            {
//                services.AddApplicationInsightsTelemetry(config.OrleansAppInsightsKey);
//            }

//            return services;
//        }





//        public static IServiceCollection AddOrleansClusterClient(this IServiceCollection services, ILoggerFactory loggerFactory,
//            Action<TcpGatewayOptions> configureOptions)
//        {
//            return services.AddOrleansClusterClient(loggerFactory, ob => ob.Configure(configureOptions));
//        }

//        public static IServiceCollection AddOrleansClusterClient(this IServiceCollection services, ILoggerFactory loggerFactory,
//            Action<Microsoft.Extensions.Options.OptionsBuilder<TcpGatewayOptions>> configureOptions)
//        {
//            configureOptions?.Invoke(services.AddOptions<TcpGatewayOptions>());
//            services.AddSingleton<ILoggerFactory>(CreateLoggerFactory);
//            services.AddSingleton<IClusterClient>(CreateClusterClient);
//            return services.AddSingleton<TcpGatewayService>();
//        }

//        private static bool HasLoggerType(OrleansConfig config, string typeName)
//        {
//            string[] loggerTypes = config.OrleansLoggerTypes?.Split(";", StringSplitOptions.RemoveEmptyEntries);
//            return !string.IsNullOrEmpty(loggerTypes.Where((t) => t.ToLowerInvariant() == typeName).FirstOrDefault());
//        }

//        private static LogLevel GetLogLevel(OrleansConfig config)
//        {
//            return Enum.Parse<LogLevel>(config.OrleansLogLevel, true);
//        }


//        private static ILoggerFactory CreateLoggerFactory(IServiceProvider serviceProvider)
//        {
//            OrleansConfig config = serviceProvider.GetService<OrleansConfig>();
//            ILoggerFactory loggerFactory = new LoggerFactory();
//            LogLevel logLevel = Enum.Parse<LogLevel>(config.OrleansLogLevel, true);
//            string[] loggerTypes = config.OrleansLoggerTypes?.Split(";", StringSplitOptions.RemoveEmptyEntries);

//            if (HasLoggerType(config, "console"))
//            {
//                loggerFactory.AddConsole(logLevel);
//            }

//            if (HasLoggerType(config, "debug"))
//            {
//                loggerFactory.AddDebug(logLevel);
//            }

//            if (HasLoggerType(config, "appinsights"))
//            {
//                AppInsightsOptions appOptions = new AppInsightsOptions()
//                {
//                    DeveloperMode = false,
//                    InstrumentationKey = config.OrleansAppInsightsKey

//                };

//                loggerFactory.AddAppInsights(appOptions);
//            }

//            return loggerFactory;
//        }

//        private static IClusterClient CreateClusterClient(IServiceProvider serviceProvider)
//        {
//            OrleansConfig config = serviceProvider.GetService<OrleansConfig>();
//            string storageType = GetStorageType(config.OrleansDataConnectionString);

//            TcpGatewayOptions options = serviceProvider.GetOptionsByName<TcpGatewayOptions>("TcpGatewayOptions");

//            if (config.Dockerized)
//            {
//                var localClient = new ClientBuilder()
//                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
//                .UseLocalhostClustering()
//                .Build();

//                localClient.Connect(RetryFilter).GetAwaiter();
//                return localClient;
//            }
//            else
//            {
//                var client = new ClientBuilder()
//                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
//                    .Configure<testME.ClusterOptions>(op =>
//                    {
//                        op.ClusterId = config.OrleansClusterId;
//                        op.ServiceId = config.OrleansServiceId;
//                    });



//                if (HasLoggerType(config, "appinsights"))
//                {
//                    client.AddApplicationInsightsTelemetryConsumer(config.OrleansAppInsightsKey);
//                }

//                if (storageType == "Redis")
//                {
//                    ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
//                    ILogger<RedisGatewayListProvider> logger = loggerFactory.CreateLogger<RedisGatewayListProvider>();
//                    client.UseRedisGatewayListProvider(logger, op =>
//                            op.ConnectionString = config.OrleansDataConnectionString
//                        );
//                }
//                else if (storageType == "AzureStorage")
//                {
//                    client.UseAzureStorageClustering(op => op.ConnectionString = config.OrleansDataConnectionString);
//                }

//                IClusterClient clusterClient = client.Build();

//                clusterClient.Connect(RetryFilter).GetAwaiter();
//                return clusterClient;
//            }
//        }

//        private static string GetStorageType(string connectionString)
//        {
//            string cs = connectionString.ToLowerInvariant();
//            if (cs.Contains(":6380") || cs.Contains(":6379"))
//            {
//                return "Redis";
//            }
//            else if (cs.Contains("defaultendpointsprotocol=") && cs.Contains("accountname=") && cs.Contains("accountkey="))
//            {
//                return "AzureStorage";
//            }
//            else
//            {
//                throw new ArgumentException("Invalid connection string");
//            }

//        }


//        private static async Task<bool> RetryFilter(Exception exception)
//        {
//            if (exception.GetType() != typeof(SiloUnavailableException))
//            {
//                Console.WriteLine($"Cluster client failed to connect to cluster with unexpected error.  Exception: {exception}");
//                return false;
//            }
//            attempt++;
//            Console.WriteLine($"Cluster client attempt {attempt} of {initializeAttemptsBeforeFailing} failed to connect to cluster.  Exception: {exception}");
//            if (attempt > initializeAttemptsBeforeFailing)
//            {
//                return false;
//            }
//            await Task.Delay(TimeSpan.FromSeconds(10));
//            return true;
//        }
//    }
//}
