namespace AM.TestTask.Data.Relational.Sync.Abstractions;

public interface ITableSynchronizer<in TSource, TEntity> 
    where TEntity : class
    where TSource : class
{
    Task SynchronizeAsync(IAsyncEnumerable<TSource> sourceStream, CancellationToken cancellationToken = default);
}