namespace AM.TestTask.Data.Cache.GenericDistributedCache.Interfaces;

public interface IGenericDistributedCache<TKey, TValue>
{
    Task<TValue?> GetValueAsync(TKey key, int[]? retryDelays = null, CancellationToken cancellationToken = default);

    Task<bool> SetValueAsync(TKey key, TValue value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    Task<bool> RemoveValueAsync(TKey key, CancellationToken cancellationToken = default);
}
