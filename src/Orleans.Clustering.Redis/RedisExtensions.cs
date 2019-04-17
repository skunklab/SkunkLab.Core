using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Messaging;
using System;

namespace Orleans.Clustering.Redis
{
    public static class RedisExtensions
    {

        #region Membership Table
        public static ISiloHostBuilder UseRedisMembership(this ISiloHostBuilder builder, ILogger<RedisMembershipTable> logger,
           Action<RedisClusteringOptions> configureOptions)
        {            
            return builder.ConfigureServices(services => services.UseRedisMembership(configureOptions));
        }

        public static ISiloHostBuilder UseRedisMembership(this ISiloHostBuilder builder,
           Action<RedisClusteringOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.UseRedisMembership(configureOptions));
        }

        public static ISiloHostBuilder UseRedisMembership(this ISiloHostBuilder builder, ILogger<RedisMembershipTable> logger,
            Action<OptionsBuilder<RedisClusteringOptions>> configureOptions)
        {
            return builder.ConfigureServices(services => services.UseRedisMembership(configureOptions));
        }

        public static ISiloHostBuilder UseRedisMembership(this ISiloHostBuilder builder, ILogger<RedisMembershipTable> logger)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddOptions<RedisClusteringOptions>();
                services.AddSingleton<IMembershipTable, RedisMembershipTable>();               
                services.TryAddSingleton<ILogger<RedisMembershipTable>>(logger);
            });
        }

        public static IServiceCollection UseRedisMembership(this IServiceCollection services,
            Action<RedisClusteringOptions> configureOptions)
        {
            return services.UseRedisMembership(ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection UseRedisMembership(this IServiceCollection services,
            Action<OptionsBuilder<RedisClusteringOptions>> configureOptions)
        {
            configureOptions?.Invoke(services.AddOptions<RedisClusteringOptions>());
            return services.AddSingleton<IMembershipTable, RedisMembershipTable>();
        }

        #endregion

        #region Gateway List Provider

        


        public static IClientBuilder UseRedisGatewayListProvider(this IClientBuilder builder, ILogger<RedisGatewayListProvider> logger, Action<RedisClusteringOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.UseRedisGatewayListProvider(configureOptions));
        }

        public static IClientBuilder UseRedisGatewayListProvider(this IClientBuilder builder, ILogger<RedisGatewayListProvider> logger)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddOptions<RedisClusteringOptions>();
                services.AddSingleton<IGatewayListProvider, RedisGatewayListProvider>();
                services.TryAddSingleton<ILogger<RedisGatewayListProvider>>(logger);
            });
        }

        public static IClientBuilder UseRedisGatewayListProvider(this IClientBuilder builder, ILogger<RedisGatewayListProvider> logger,  
            Action<OptionsBuilder<RedisClusteringOptions>> configureOptions)
        {
            return builder.ConfigureServices(services => services.UseRedisGatewayListProvider(configureOptions));
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

        #endregion
    }
}
