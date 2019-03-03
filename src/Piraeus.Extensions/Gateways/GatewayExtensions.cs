using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Piraeus.Extensions.Configuration;
using Piraeus.Extensions.Logging;
using Piraeus.Extensions.Options;
using Piraeus.GrainInterfaces;
using System;
using System.Threading.Tasks;
using OrleansConfiguration = Orleans.Configuration;

namespace Piraeus.Extensions.Gateways
{
    public static class GatewayExtensions
    {
        private static int attempt;
        private static readonly int initializeAttemptsBeforeFailing = 12;

        public static IServiceCollection AddGatewayService(IServiceCollection services, Type serviceType, Action<PiraeusGatewayOptions> configureOptions)
        {
            return services.AddOrleansClusterClient(serviceType, ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection AddGatewayService(this IServiceCollection services, Type serviceType,
           Action<Microsoft.Extensions.Options.OptionsBuilder<PiraeusGatewayOptions>> configureOptions)
        {
            configureOptions?.Invoke(services.AddOptions<PiraeusGatewayOptions>());    
            
            return services;
        }

        public static IServiceCollection AddOrleansClusterClient(this IServiceCollection services, Type serviceType,
            Action<Microsoft.Extensions.Options.OptionsBuilder<PiraeusGatewayOptions>> configureOptions)
        {
            configureOptions?.Invoke(services.AddOptions<PiraeusGatewayOptions>());
            //create the clustering provider and cluster client
            services.AddSingleton<IClusterClient>(CreateClusterClient);
            //create the service as singleton
            return services.AddSingleton(serviceType);
        }

        private static IClusterClient CreateClusterClient(IServiceProvider serviceProvider)
        {
            PiraeusGatewayOptions options = serviceProvider.GetOptionsByName<PiraeusGatewayOptions>("PiraeusGatewayOptions");           
           
            if (!options.Dockerized)
            {
                var localClient = new ClientBuilder()
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPiSystem).Assembly))
                .UseLocalhostClustering()
                .AddLoggers(options)
                .Build();

                localClient.Connect(RetryFilter).GetAwaiter();
                
                return localClient;
            }
            else
            {
                var client = new ClientBuilder()
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPiSystem).Assembly))
                    .Configure<OrleansConfiguration.ClusterOptions>(op =>
                    {
                        op.ClusterId = options.ClusterId;
                        op.ServiceId = options.ServiceId;
                    });

                client.AddLoggers(options);
                if (options.StorageType == OrleansStorageType.Redis)
                    client.UseRedisGatewayListProvider(op => op.ConnectionString = options.DataConnectionString);
                if (options.StorageType == OrleansStorageType.AzureStorage)
                    client.UseAzureStorageClustering(op => op.ConnectionString = options.DataConnectionString);
                
                IClusterClient clusterClient = client.Build();

                clusterClient.Connect(RetryFilter).GetAwaiter();
                return clusterClient;
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
                attempt = 0;
                return false;
            }
            await Task.Delay(TimeSpan.FromSeconds(5));
            return true;
        }

    }
}
