using AM.TestTask.Data.Cache.GenericDistributedCache.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AM.TestTask.Data.Cache.GenericDistributedCache;

internal class GenericDistributedCache<TKey, TValue> : IGenericDistributedCache<TKey, TValue>
{
    private protected readonly IDistributedCache _distributedCache;
    private protected static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        IncludeFields = true
    };

    public GenericDistributedCache(IDistributedCache distributedCache) => _distributedCache = distributedCache;

    public virtual async Task<TValue?> GetValueAsync(TKey key, int[]? retryDelays = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = ToCacheKey(key);

        if (retryDelays is null || retryDelays.Length == 0)
        {
            var data = await _distributedCache.GetStringAsync(cacheKey);
            return data != null ? JsonSerializer.Deserialize<TValue>(data, _jsonOptions) : default;
        }

        foreach (var delay in retryDelays)
        {
            var data = await _distributedCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(data))
                return JsonSerializer.Deserialize<TValue>(data, _jsonOptions);

            await Task.Delay(delay);
        }

        return default;
    }

    public virtual async Task<bool> RemoveValueAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var cacheKey = ToCacheKey(key);
        await _distributedCache.RemoveAsync(cacheKey);

        return true;
    }

    public virtual async Task<bool> SetValueAsync(TKey key, TValue value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = ToCacheKey(key);
        var json = JsonSerializer.Serialize(value, _jsonOptions);

        var options = new DistributedCacheEntryOptions();

        if (expiry.HasValue && expiry.Value > TimeSpan.Zero)
            options.AbsoluteExpirationRelativeToNow = expiry;

        await _distributedCache.SetStringAsync(cacheKey, json, options);

        return true;
    }

    private protected static string ToCacheKey(TKey key)
    {
        return key switch
        {
            string s => s,
            int i => i.ToString(),
            Guid g => g.ToString(),
            _ => JsonSerializer.Serialize(key, _jsonOptions)
        };
    }
}

