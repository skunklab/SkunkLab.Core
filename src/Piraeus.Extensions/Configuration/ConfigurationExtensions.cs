using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Clustering.Redis;
using Orleans.Messaging;
using Piraeus.Configuration.Core;
using Piraeus.Configuration.Settings;
using Piraeus.Extensions.Options;
using System;

namespace Piraeus.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddOrleansConfiguration(this IConfigurationBuilder configure, out OrleansConfig orleansConfig)
        {
            configure.AddJsonFile(Environment.CurrentDirectory + "\\orleansconfig.json")
                .AddEnvironmentVariables("OR_");
            IConfigurationRoot root = configure.Build();
            orleansConfig = new OrleansConfig();
            ConfigurationBinder.Bind(root, orleansConfig);

            return configure;
        }

        public static IConfigurationBuilder AddPiraeusConfiguration(this IConfigurationBuilder configure, out PiraeusConfig piraeusConfig)
        {
            configure.AddJsonFile(Environment.CurrentDirectory + "\\piraeusconfig.json")
                .AddEnvironmentVariables("PI_");
            IConfigurationRoot root = configure.Build();
            piraeusConfig = new PiraeusConfig();
            ConfigurationBinder.Bind(root, piraeusConfig);

            return configure;
        }
               
        public static IClientBuilder UseRedisGatewayListProvider(this IClientBuilder builder, Action<RedisClusteringOptions> configureOptions)
        {            
            return builder.ConfigureServices(services => services.UseRedisGatewayListProvider(configureOptions));
        }

        public static IClientBuilder UseRedisGatewayListProvider(this IClientBuilder builder, Action<OptionsBuilder<RedisClusteringOptions>> configureOptions)
        {
            return builder.ConfigureServices(services => services.UseRedisGatewayListProvider(configureOptions));
        }

        public static IClientBuilder UseRedisGatewayListProvider(this IClientBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddOptions<RedisClusteringOptions>();
                services.AddSingleton<IGatewayListProvider, RedisGatewayListProvider>();
            });
        }

        public static IServiceCollection UseRedisGatewayListProvider(this IServiceCollection services,
            Action<RedisClusteringOptions> configureOptions)
        {
            return services.UseRedisGatewayListProvider(ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection UseRedisGatewayListProvider(this IServiceCollection services,
            Action<OptionsBuilder<RedisClusteringOptions>> configureOptions)
        {
            configureOptions?.Invoke(services.AddOptions<RedisClusteringOptions>());
            return services.AddSingleton<IGatewayListProvider, RedisGatewayListProvider>();
        }


    }
}
