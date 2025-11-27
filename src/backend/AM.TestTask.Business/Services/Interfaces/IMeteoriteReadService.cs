using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Enums;
using AM.TestTask.Business.Models;

namespace AM.TestTask.Business.Services.Interfaces;

public interface IMeteoriteReadService
{
    public Task<IEnumerable<MeteoriteStatisticDto>> GetStatistic(
        MeteoriteStatisticFilter filter,
        StatisticSortBy sortBy = StatisticSortBy.Year,
        SortDirection sortDir = SortDirection.Asc,
        CancellationToken cancellationToken = default);

    public Task<IEnumerable<RecClassDto>> GetClasses(CancellationToken cancellationToken = default);
}
