using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Clustering.Redis;
using Orleans.Messaging;
using Piraeus.Configuration;
using System;

namespace Piraeus.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddOrleansConfiguration(this IServiceCollection services)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("./orleansconfig.json")
                .AddEnvironmentVariables("OR_");
            IConfigurationRoot root = builder.Build();
            OrleansConfig config = new OrleansConfig();
            ConfigurationBinder.Bind(root, config);
            services.AddSingleton<OrleansConfig>(config);

            return services;
        }

        public static IServiceCollection AddOrleansConfiguration(this IServiceCollection services, out OrleansConfig config)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("./orleansconfig.json")
                .AddEnvironmentVariables("OR_");
            IConfigurationRoot root = builder.Build();
            config = new OrleansConfig();
            ConfigurationBinder.Bind(root, config);
            services.AddSingleton<OrleansConfig>(config);

            return services;
        }
        public static IServiceCollection AddPiraeusConfiguration(this IServiceCollection services)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("./piraeusconfig.json")
                .AddEnvironmentVariables("PI_");
            IConfigurationRoot root = builder.Build();
            PiraeusConfig config = new PiraeusConfig();
            ConfigurationBinder.Bind(root, config);
            services.AddSingleton<PiraeusConfig>(config);

            return services;
        }

        public static IServiceCollection AddPiraeusConfiguration(this IServiceCollection services, out PiraeusConfig config)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("./piraeusconfig.json")
                .AddEnvironmentVariables("PI_");
            IConfigurationRoot root = builder.Build();
            config = new PiraeusConfig();
            ConfigurationBinder.Bind(root, config);
            services.AddSingleton<PiraeusConfig>(config);

            return services;
        }
        public static IConfigurationBuilder AddOrleansConfiguration(this IConfigurationBuilder configure, out OrleansConfig orleansConfig)
        {
            configure.AddJsonFile("./orleansconfig.json")
                .AddEnvironmentVariables("OR_");
            IConfigurationRoot root = configure.Build();
            orleansConfig = new OrleansConfig();
            ConfigurationBinder.Bind(root, orleansConfig);
            return configure;
        }

        public static IConfigurationBuilder AddPiraeusConfiguration(this IConfigurationBuilder configure, out PiraeusConfig piraeusConfig)
        {
            configure.AddJsonFile("./piraeusconfig.json")
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
