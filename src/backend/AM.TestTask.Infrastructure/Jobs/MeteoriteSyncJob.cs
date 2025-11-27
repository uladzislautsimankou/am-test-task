using AM.TestTask.Business.Exceptions;
using AM.TestTask.Business.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace AM.TestTask.Infrastructure.Jobs;

internal sealed class MeteoriteSyncJob(
    IServiceProvider serviceProvider,
    ILogger<MeteoriteSyncJob> logger
    ) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting sync job...");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IMeteoriteSyncService>();

            await syncService.SynchronizeMeteoritesAsync(context.CancellationToken);

            logger.LogInformation("Sync job finished.");
        }
        catch (SynchronizationException ex)
        {
            logger.LogError("Job failed to synchronize data. Reason: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Job crashed with an unexpected error.");
        }
    }
}
