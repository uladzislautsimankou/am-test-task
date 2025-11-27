using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Models;

namespace AM.TestTask.Business.Cache;

public interface IMeteoriteDataCache
{
    Task<IEnumerable<MeteoriteStatisticDto>?> GetStatsAsync(MeteoriteStatisticFilter filter, CancellationToken cancellationToken = default);

    Task SetStatsAsync(MeteoriteStatisticFilter filter, IEnumerable<MeteoriteStatisticDto> data, CancellationToken cancellationToken = default);

    Task<IEnumerable<RecClassDto>?> GetClassesAsync(CancellationToken cancellationToken = default);

    Task SetClassesAsync(IEnumerable<RecClassDto> data, CancellationToken cancellationToken = default);

    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
