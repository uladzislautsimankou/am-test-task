namespace AM.TestTask.Business.Services.Interfaces;

public interface IMeteoriteSyncService
{
    public Task SynchronizeMeteoritesAsync(CancellationToken cancellationToken = default);
}
