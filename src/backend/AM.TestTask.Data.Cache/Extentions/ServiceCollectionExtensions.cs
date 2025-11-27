using AM.TestTask.Business.Cache;
using AM.TestTask.Data.Cache.Cache;
using AM.TestTask.Data.Cache.GenericDistributedCache;
using AM.TestTask.Data.Cache.GenericDistributedCache.Interfaces;
using AM.TestTask.Data.Cache.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AM.TestTask.Data.Cache.Extentions;

public static class ServiceCollectionExtensions
{
    public static void AddDataCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CacheOptions>(configuration.GetSection("Cache"));

        var cacheOptions = new CacheOptions();
        configuration.GetSection("Cache").Bind(cacheOptions);

        switch (cacheOptions.Provider.ToLowerInvariant())
        {
            case "redis":
                if (cacheOptions.Redis == null)
                    throw new Exception("Redis provider selected but Redis options are missing.");

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = cacheOptions.Redis.Configuration;
                    options.InstanceName = cacheOptions.Redis.InstanceName;
                });
                break;

            case "inmemory":
            default:
                services.AddDistributedMemoryCache();
                break;
        }

        services.AddSingleton(typeof(IGenericDistributedCache<,>), typeof(GenericDistributedCache<,>));
        services.AddScoped<IMeteoriteDataCache, MeteoriteDataCache>();
    }
}
