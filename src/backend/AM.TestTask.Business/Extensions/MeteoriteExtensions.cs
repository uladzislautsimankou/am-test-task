using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Enums;

namespace AM.TestTask.Business.Extensions;

public static class MeteoriteExtensions
{
    public static IQueryable<MeteoriteStatisticDto> ApplySorting(
        this IQueryable<MeteoriteStatisticDto> query,
        StatisticSortBy field = StatisticSortBy.Year,
        SortDirection dir = SortDirection.Asc)
    {
        return (field, dir) switch
        {
            (StatisticSortBy.Year, SortDirection.Asc) => query.OrderBy(x => x.Year),
            (StatisticSortBy.Year, SortDirection.Desc) => query.OrderByDescending(x => x.Year),

            (StatisticSortBy.Count, SortDirection.Asc) => query.OrderBy(x => x.Count),
            (StatisticSortBy.Count, SortDirection.Desc) => query.OrderByDescending(x => x.Count),

            (StatisticSortBy.Mass, SortDirection.Asc) => query.OrderBy(x => x.TotalMass),
            (StatisticSortBy.Mass, SortDirection.Desc) => query.OrderByDescending(x => x.TotalMass),

            _ => query.OrderBy(x => x.Year)
        };
    }

    public static IEnumerable<MeteoriteStatisticDto> ApplySorting(
        this IEnumerable<MeteoriteStatisticDto> data,
        StatisticSortBy field = StatisticSortBy.Year,
        SortDirection dir = SortDirection.Asc)
    {
        return (field, dir) switch
        {
            (StatisticSortBy.Year, SortDirection.Asc) => data.OrderBy(x => x.Year),
            (StatisticSortBy.Year, SortDirection.Desc) => data.OrderByDescending(x => x.Year),

            (StatisticSortBy.Count, SortDirection.Asc) => data.OrderBy(x => x.Count),
            (StatisticSortBy.Count, SortDirection.Desc) => data.OrderByDescending(x => x.Count),

            (StatisticSortBy.Mass, SortDirection.Asc) => data.OrderBy(x => x.TotalMass),
            (StatisticSortBy.Mass, SortDirection.Desc) => data.OrderByDescending(x => x.TotalMass),

            _ => data.OrderBy(x => x.Year)
        };
    }
}
