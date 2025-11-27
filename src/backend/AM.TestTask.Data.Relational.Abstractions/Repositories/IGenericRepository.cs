using System.Linq.Expressions;

namespace AM.TestTask.Data.Relational.Abstractions.Repositories;

public interface IGenericRepository<TEntity, TId> : IGenericReadonlyRepository<TEntity, TId>
     where TEntity : class, IIdentifiablyEntity<TId>
{
    void Create(TEntity entity);

    void CreateRange(IEnumerable<TEntity> entities);

    void Update(TEntity entity, params Expression<Func<TEntity, object?>>[] updatedProperties);

    void Delete(TEntity entity);

    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
}