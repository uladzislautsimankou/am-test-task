using AM.TestTask.Business.Cache;
using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Models;
using AM.TestTask.Data.Cache.GenericDistributedCache.Interfaces;
using AM.TestTask.Data.Cache.Options;
using Microsoft.Extensions.Options;

namespace AM.TestTask.Data.Cache.Cache;

internal sealed class MeteoriteDataCache(
    IGenericDistributedCache<string, IEnumerable<MeteoriteStatisticDto>> statsCache,
    IGenericDistributedCache<string, IEnumerable<RecClassDto>> classesCache,
    IGenericDistributedCache<string, int> versionCache,
    IOptionsSnapshot<CacheOptions> options
    ) : IMeteoriteDataCache
{
    private const string VersionKey = "meteorite:stats:current_version";
    private const string ClassesKeySuffix = "dictionaries:classes";
    private readonly int _ttlMinutes = options.Value.TtlMinutes;

    private static string MakeStatsKey(MeteoriteStatisticFilter filter, int version)
        => $"v{version}:stats:{filter.YearFrom}-{filter.YearTo}-{filter.RecClassId}-{filter.NamePart}";

    public async Task<IEnumerable<MeteoriteStatisticDto>?> GetStatsAsync(MeteoriteStatisticFilter filter, CancellationToken cancellationToken = default)
    {
        var version = await GetCurrentVersionAsync(cancellationToken);
        var key = MakeStatsKey(filter, version);

        return await statsCache.GetValueAsync(key, cancellationToken: cancellationToken);
    }

    public async Task SetStatsAsync(MeteoriteStatisticFilter filter, IEnumerable<MeteoriteStatisticDto> data, CancellationToken cancellationToken = default)
    {
        var version = await GetCurrentVersionAsync(cancellationToken);
        var key = MakeStatsKey(filter, version);

        await statsCache.SetValueAsync(key, data, TimeSpan.FromMinutes(_ttlMinutes), cancellationToken);
    }

    public async Task<IEnumerable<RecClassDto>?> GetClassesAsync(CancellationToken cancellationToken = default)
    {
        var version = await GetCurrentVersionAsync(cancellationToken);
        var key = $"v{version}:{ClassesKeySuffix}";

        return await classesCache.GetValueAsync(key, cancellationToken: cancellationToken);
    }

    public async Task SetClassesAsync(IEnumerable<RecClassDto> data, CancellationToken cancellationToken = default)
    {
        var version = await GetCurrentVersionAsync(cancellationToken);
        var key = $"v{version}:{ClassesKeySuffix}";

        await classesCache.SetValueAsync(key, data, TimeSpan.FromHours(_ttlMinutes), cancellationToken);
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        var currentVersion = await GetCurrentVersionAsync(cancellationToken);

        await versionCache.SetValueAsync(VersionKey, currentVersion + 1, TimeSpan.FromDays(30), cancellationToken);
    }

    private async Task<int> GetCurrentVersionAsync(CancellationToken cancellationToken) 
        => await versionCache.GetValueAsync(VersionKey, cancellationToken: cancellationToken);
}
