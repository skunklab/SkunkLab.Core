using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Clustering.Redis;
using Orleans.Configuration;
using Orleans.Hosting;
using Piraeus.Configuration;
using Piraeus.GrainInterfaces;
using System;
using System.Threading.Tasks;


namespace Piraeus.Extensions.Orleans
{
    public static class OrleansExtensions
    {
        #region Cluster Client
        public static void AddSingletonOrleansClusterClient(this IServiceCollection services, OrleansConfig config)
        {
            services.AddSingleton<IClusterClient>(serviceProvider =>
            {
                var builder = new ClientBuilder();
                builder.AddOrleansClusterClient(config);
                IClusterClient client = builder.Build();
                client.Connect(CreateRetryFilter()).GetAwaiter().GetResult();
                return client;
            });
        }

        public static void AddScopedOrleansClusterClient(this IServiceCollection services, OrleansConfig config)
        {
            services.AddScoped<IClusterClient>(serviceProvider =>
            {
                var builder = new ClientBuilder();
                builder.AddOrleansClusterClient(config);
                IClusterClient client = builder.Build();
                client.Connect(CreateRetryFilter()).GetAwaiter().GetResult();
                return client;
            });
        }

        public static void AddTransientOrleansClusterClient(this IServiceCollection services, OrleansConfig config)
        {
            services.AddTransient<IClusterClient>(serviceProvider =>
            {
                var builder = new ClientBuilder();
                builder.AddOrleansClusterClient(config);
                IClusterClient client = builder.Build();
                client.Connect(CreateRetryFilter()).GetAwaiter().GetResult();
                return client;
            });
        }

        private static IClientBuilder AddOrleansClusterClient(this IClientBuilder builder, OrleansConfig config)
        {
            builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPiSystem).Assembly));

#if DEBUG
            builder.UseLocalhostClustering();
#else
            builder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = config.ClusterId;
                options.ServiceId = config.ServiceId;
            });

            if (config.DataConnectionString.Contains("6379") || config.DataConnectionString.Contains("6380"))
            {
                builder.UseRedisGatewayListProvider(options => options.ConnectionString = config.DataConnectionString);
            }
            else
            {
                builder.UseAzureStorageClustering(options => options.ConnectionString = config.DataConnectionString);
            }
#endif

            //if (!config.Dockerized)
            //{
            //    builder.UseLocalhostClustering();
            //}
            //else
            //{
            //    builder.Configure<ClusterOptions>(options =>
            //    {
            //        options.ClusterId = config.ClusterId;
            //        options.ServiceId = config.ServiceId;
            //    });

            //    if (config.DataConnectionString.Contains("6379") || config.DataConnectionString.Contains("6380"))
            //    {
            //        builder.UseRedisGatewayListProvider(options => options.ConnectionString = config.DataConnectionString);
            //    }
            //    else
            //    {   
            //        builder.UseAzureStorageClustering(options => options.ConnectionString = config.DataConnectionString);
            //    }
            //}

            Piraeus.Configuration.LoggerType loggers = config.GetLoggerTypes();

            if (loggers.HasFlag(Piraeus.Configuration.LoggerType.AppInsights))
            {
                builder.AddApplicationInsightsTelemetryConsumer(config.AppInsightsKey);
            }

            builder.ConfigureLogging(op =>
            {
                if (loggers.HasFlag(Piraeus.Configuration.LoggerType.AppInsights))
                {
                    op.AddApplicationInsights(config.AppInsightsKey);
                    op.SetMinimumLevel(Enum.Parse<LogLevel>(config.LogLevel, true));
                }

                if (loggers.HasFlag(Piraeus.Configuration.LoggerType.Console))
                {
                    op.AddConsole();
                    op.SetMinimumLevel(Enum.Parse<LogLevel>(config.LogLevel, true));
                }

                if (loggers.HasFlag(Piraeus.Configuration.LoggerType.Debug))
                {
                    op.AddDebug();
                    op.SetMinimumLevel(Enum.Parse<LogLevel>(config.LogLevel, true));
                }
            });

            return builder;
        }

        private static Func<Exception, Task<bool>> CreateRetryFilter(int maxAttempts = 5)
        {
            var attempt = 0;
            return RetryFilter;

            async Task<bool> RetryFilter(Exception exception)
            {
                attempt++;
                Console.WriteLine($"Cluster client attempt {attempt} of {maxAttempts} failed to connect to cluster.  Exception: {exception}");
                if (attempt > maxAttempts)
                {
                    return false;
                }

                await Task.Delay(TimeSpan.FromSeconds(4));
                return true;
            }
        }
#endregion
    }
}
