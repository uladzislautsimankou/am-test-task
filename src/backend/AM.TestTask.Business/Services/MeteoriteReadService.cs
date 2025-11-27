using AM.TestTask.Business.Cache;
using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Enums;
using AM.TestTask.Business.Exceptions;
using AM.TestTask.Business.Extensions;
using AM.TestTask.Business.Models;
using AM.TestTask.Business.Repositories;
using AM.TestTask.Business.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AM.TestTask.Business.Services;

internal sealed class MeteoriteReadService(
    IMeteoriteReadOnlyRepository meteoriteRepository,
    IMeteoriteDataCache meteoriteDataCache,
    IRecClassReadOnlyRepository recClassRepository,
    IValidator<MeteoriteStatisticFilter> meteoriteStatiscticValidator,
    ILogger<MeteoriteReadService> logger
    ) : IMeteoriteReadService
{
    public async Task<IEnumerable<RecClassDto>> GetClasses(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Fetching meteorite classification list...");

            var cached = await meteoriteDataCache.GetClassesAsync(cancellationToken);

            if (cached is not null)
            {
                var cachedList = cached.ToList();

                logger.LogInformation("Meteorite classifications retrieved from CACHE. Count: {Count}", cachedList.Count);

                return cachedList;
            }

            var classes = await recClassRepository.GetAsProjectionAsync(
                projection: x => new RecClassDto(x.Id, x.Name),
                orderBy: q => q.OrderBy(x => x.Name),
                cancellationToken: cancellationToken);

            logger.LogInformation("Meteorite classifications retrieved from DATABASE. Count: {Count}", classes.Count);

            await meteoriteDataCache.SetClassesAsync(classes, cancellationToken); 

            return classes;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve meteorite classifications.");

            throw new DataRetrievalException("Could not load classifications.", ex);
        }
    }

    public async Task<IEnumerable<MeteoriteStatisticDto>> GetStatistic(
        MeteoriteStatisticFilter filter,
        StatisticSortBy sortBy = StatisticSortBy.Year,
        SortDirection sortDir = SortDirection.Asc,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Fetching meteorite statistics. Filter: {@Filter}", filter);

            await meteoriteStatiscticValidator.ValidateAndThrowAsync(filter, cancellationToken);

            var cached = await meteoriteDataCache.GetStatsAsync(filter, cancellationToken);

            if (cached is not null)
            {
                var cachedList = cached
                    .ApplySorting(sortBy, sortDir)
                    .ToList();

                logger.LogInformation("Statistics retrieved from CACHE. Filter: {@Filter}. Rows returned: {Count}", filter, cachedList.Count);

                return cachedList;
            }

            var statistic = await meteoriteRepository.GetStatsByYearAsync(filter, sortBy, sortDir, cancellationToken);

            await meteoriteDataCache.SetStatsAsync(filter, statistic, cancellationToken);

            logger.LogInformation("Statistics retrieved from DATABASE. Filter: {@Filter}. Rows returned: {Count}", filter, statistic.Count);

            return statistic;
        }
        catch (ValidationException ex)
        {
            logger.LogError(ex, "Failed to filters: {@Filter}", filter);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to calculate meteorite statistics with filter: {@Filter}", filter);

            throw new DataRetrievalException("Could not calculate statistics.");
        }
    }
}