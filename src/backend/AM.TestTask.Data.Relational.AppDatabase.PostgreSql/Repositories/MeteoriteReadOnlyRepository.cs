using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Entities;
using AM.TestTask.Business.Enums;
using AM.TestTask.Business.Extensions;
using AM.TestTask.Business.Models;
using AM.TestTask.Business.Repositories;
using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Context;
using AM.TestTask.Data.Relational.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Repositories;

internal sealed class MeteoriteReadOnlyRepository(ApplicationDatabaseContext context) :
    EntityFrameworkGenericReadonlyRepository<Meteorite, long>(context),
    IMeteoriteReadOnlyRepository
{
    public async Task<List<MeteoriteStatisticDto>> GetStatsByYearAsync(
        MeteoriteStatisticFilter filter,
        StatisticSortBy sortBy = StatisticSortBy.Year,
        SortDirection sortDir = SortDirection.Asc,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(filter: x =>
            (filter.YearFrom == null || x.Year >= filter.YearFrom)
            && (filter.YearTo == null || x.Year <= filter.YearTo)
            && (filter.RecClassId == null || x.RecClassId == filter.RecClassId)
            && (string.IsNullOrEmpty(filter.NamePart) || EF.Functions.ILike(x.Name, $"%{filter.NamePart}%")));

        return await query
            .GroupBy(m => m.Year)
            .Select(g => new MeteoriteStatisticDto
            {
                Year = g.Key,
                Count = g.Count(),
                TotalMass = g.Sum(x => x.Mass) ?? 0
            })
            .ApplySorting(sortBy, sortDir)
            .ToListAsync(cancellationToken);
    }
}

