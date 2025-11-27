using AM.TestTask.Data.Relational.Abstractions;
using AM.TestTask.Data.Relational.Abstractions.Repositories;
using AM.TestTask.Data.Relational.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AM.TestTask.Data.Relational.EntityFramework.Repositories;

public class EntityFrameworkGenericReadonlyRepository<TEntity, TId> :
    IGenericReadonlyRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>
{
    private protected readonly BaseDbContext _context;

    public EntityFrameworkGenericReadonlyRepository(BaseDbContext context) => _context = context;

    public async Task<TEntity?> GetByIdAsync(
        TId id,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        bool withoutTracking = true,
        bool ignoreGlobalFilters = false,
        CancellationToken cancellationToken = default)
        => await GetQueryable(x => x.Id!.Equals(id), null, includeMany, include, null, null, withoutTracking, ignoreGlobalFilters)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        bool withoutTracking = true,
        bool ignoreGlobalFilters = false,
        CancellationToken cancellationToken = default)
        => await GetQueryable(filter, null, includeMany, include, null, null, withoutTracking, ignoreGlobalFilters)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        int? skip = null,
        int? take = null,
        bool withoutTracking = true,
        bool ignoreGlobalFilters = false,
        CancellationToken cancellationToken = default)
        => await GetQueryable(filter, orderBy, includeMany, include, skip, take, withoutTracking, ignoreGlobalFilters)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TProjection>> GetAsProjectionAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        int? skip = null,
        int? take = null,
        bool withoutTracking = true,
        bool ignoreGlobalFilters = false,
        CancellationToken cancellationToken = default)
        => await GetQueryable(filter, orderBy, includeMany, include, skip, take, withoutTracking, ignoreGlobalFilters)
            .Select(projection)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> filter,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        bool ignoreGlobalFilters = false,
        CancellationToken cancellationToken = default)
        => await GetQueryable(filter, null, includeMany, include, null, null, true, ignoreGlobalFilters).AnyAsync(cancellationToken);

    protected virtual IQueryable<TEntity> GetQueryable(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        int? skip = null,
        int? take = null,
        bool withoutTracking = true,
        bool ignoreGlobalFilters = false)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        query = withoutTracking ? query.AsNoTrackingWithIdentityResolution() : query.AsTracking();

        if (filter != null) query = query.Where(filter);

        if (include != null) query = query.Include(include);

        if (!string.IsNullOrEmpty(includeMany))
        {
            query = includeMany
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query,
                    (current, property) => current.Include(property.Trim()));
        }

        if (orderBy != null) query = orderBy(query);

        if (skip != null) query = query.Skip(skip.Value);

        if (take != null) query = query.Take(take.Value);

        if (ignoreGlobalFilters) query = query.IgnoreQueryFilters();

        return query;
    }
}

