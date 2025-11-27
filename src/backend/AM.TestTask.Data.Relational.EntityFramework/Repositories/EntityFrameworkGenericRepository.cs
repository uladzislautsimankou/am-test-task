using AM.TestTask.Data.Relational.Abstractions;
using AM.TestTask.Data.Relational.Abstractions.Repositories;
using AM.TestTask.Data.Relational.EntityFramework.Context;
using System.Linq.Expressions;

namespace AM.TestTask.Data.Relational.EntityFramework.Repositories;

public class EntityFrameworkGenericRepository<TEntity, TId> :
    EntityFrameworkGenericReadonlyRepository<TEntity, TId>,
    IGenericRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>
{

    public EntityFrameworkGenericRepository(BaseDbContext context) : base(context) { }

    public void Create(TEntity entity) => _context.Set<TEntity>().Add(entity);

    public void CreateRange(IEnumerable<TEntity> entities) => _context.Set<TEntity>().AddRange(entities);

    public void Delete(TEntity entity) => _context.Set<TEntity>().Remove(entity);

    public void Update(TEntity entity, params Expression<Func<TEntity, object?>>[] updatedProperties)
    {
        var dbEntityEntry = _context.Entry(entity);

        if (!updatedProperties.Any())
        {
            _context.Set<TEntity>().Update(entity);
            return;
        }

        foreach (var property in updatedProperties)
        {
            dbEntityEntry.Property(property).IsModified = true;
        }
    }

    public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken: cancellationToken);

        if (entity != null) Delete(entity);
    }
}