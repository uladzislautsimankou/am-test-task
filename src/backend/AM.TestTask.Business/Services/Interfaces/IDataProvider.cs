namespace AM.TestTask.Business.Services.Interfaces;

public interface IDataProvider<TEntity> where TEntity : class
{
    public IAsyncEnumerable<TEntity> GetDataStreamAsync(CancellationToken cancellationToken = default);
}