using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using System;

namespace Orleans.Storage.Redis
{
    public static class RedisStorageExtensions
    {
        public const string DEFAULT_STORAGE_PROVIDER_NAME = "Default";

        public static ISiloHostBuilder AddRedisGrainStorage(this ISiloHostBuilder builder, string name, ILogger logger, Action<RedisStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddRedisGrainStorage(name, logger, configureOptions));
        }

        public static IServiceCollection AddRedisGrainStorage(this IServiceCollection services, string name, ILogger logger, Action<RedisStorageOptions> configureOptions)
        {          
            return services.AddRedisGrainStorage(name, logger, ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection AddRedisGrainStorage(this IServiceCollection services, string name, ILogger logger,
            Action<OptionsBuilder<RedisStorageOptions>> configureOptions = null)
        {
            configureOptions?.Invoke(services.AddOptions<RedisStorageOptions>(name));            
            services.ConfigureNamedOptionForLogging<RedisStorageOptions>(name);
            services.TryAddSingleton<IGrainStorage>(sp => sp.GetServiceByName<IGrainStorage>(DEFAULT_STORAGE_PROVIDER_NAME));
            services.TryAddSingleton<ILogger>(logger);
            //services.TryAddSingleton<IGrainStorage>(sp => sp.GetServiceByName<IGrainStorage>(name));
            return services.AddSingletonNamedService<IGrainStorage>(name, RedisGrainStorageFactory.Create)
                           .AddSingletonNamedService<ILifecycleParticipant<ISiloLifecycle>>(name, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
        }

    }
}
