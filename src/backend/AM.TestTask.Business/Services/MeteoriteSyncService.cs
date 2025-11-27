using AM.TestTask.Business.Cache;
using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Exceptions;
using AM.TestTask.Business.Services.Interfaces;
using AM.TestTask.Business.Synchronizers;
using Microsoft.Extensions.Logging;

namespace AM.TestTask.Business.Services;

internal sealed class MeteoriteSyncService(
    IDataProvider<MeteoriteDto> dataProvider,
    IMeteoriteDataCache meteoriteDataCache,
    IMeteoriteTableSychronyzer meteoriteTableSychronyzer,
    ILogger<MeteoriteSyncService> logger
    ) : IMeteoriteSyncService
{
    public async Task SynchronizeMeteoritesAsync(CancellationToken cancellationToken = default)
    {
        const int MaxRetries = 5;
        const int DelayMilliseconds = 2000;

        // предпологаю, что синхронизация должна быть полная, и только часть данных нас не устраивает.
        // поэтому если где-то что-то пошло не так, то все откатываем и делаем заново
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Starting meteorite synchronization workflow. Attempt {Attempt}/{MaxRetries}.", attempt, MaxRetries);

                var dtoStream = dataProvider.GetDataStreamAsync(cancellationToken);

                await meteoriteTableSychronyzer.SynchronizeAsync(dtoStream, cancellationToken);

                logger.LogInformation("Meteorite synchronization completed successfully on attempt {Attempt}.", attempt);

                break;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (attempt == MaxRetries)
                {
                    logger.LogError(ex, "Meteorite synchronization failed after {MaxRetries} attempts.", MaxRetries);

                    throw new SynchronizationException("Failed to synchronize meteorites after multiple attempts.");
                }

                logger.LogWarning(ex, "Attempt {Attempt} failed. Retrying in {Delay}ms...", attempt, DelayMilliseconds);

                await Task.Delay(DelayMilliseconds, cancellationToken);
            }
        }

        try
        {
            logger.LogInformation("Invalidating application cache versions...");

            await meteoriteDataCache.InvalidateAsync(cancellationToken);

            logger.LogInformation("Cache invalidated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database synchronized successfully, but failed to invalidate cache. Old data might be served until TTL expires.");
        }
    }
}