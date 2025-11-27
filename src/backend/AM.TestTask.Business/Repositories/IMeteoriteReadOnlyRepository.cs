using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Entities;
using AM.TestTask.Business.Enums;
using AM.TestTask.Business.Models;
using AM.TestTask.Data.Relational.Abstractions.Repositories;

namespace AM.TestTask.Business.Repositories;

public interface IMeteoriteReadOnlyRepository : IGenericReadonlyRepository<Meteorite, long>
{
    public Task<List<MeteoriteStatisticDto>> GetStatsByYearAsync(
        MeteoriteStatisticFilter filter,
        StatisticSortBy sortBy = StatisticSortBy.Year,
        SortDirection sortDir = SortDirection.Asc,
        CancellationToken cancellationToken = default);
}
