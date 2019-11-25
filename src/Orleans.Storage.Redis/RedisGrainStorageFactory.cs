using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Orleans.Storage.Redis
{

    public static class RedisGrainStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            IOptionsSnapshot<RedisStorageOptions> optionsSnapshot = services.GetRequiredService<IOptionsSnapshot<RedisStorageOptions>>();
            return ActivatorUtilities.CreateInstance<RedisGrainStorage>(services, name, optionsSnapshot.Get(name));
        }
    }
}
