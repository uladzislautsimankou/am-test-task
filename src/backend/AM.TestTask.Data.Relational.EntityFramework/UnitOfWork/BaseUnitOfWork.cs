using AM.TestTask.Data.Relational.Abstractions.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AM.TestTask.Data.Relational.EntityFramework.UnitOfWork;

public class BaseUnitOfWork(DbContext context, ILogger<BaseUnitOfWork> logger) : IBaseUnitOfWork
{
    public async Task CommitChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync();
            // Detach all tracked entities to prevent conflicts when re‐loading or attaching the same entities again
            context.ChangeTracker.Clear();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict occurred while committing changes.");
            throw;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while committing changes.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while committing changes.");
            throw;
        }
    }
}
