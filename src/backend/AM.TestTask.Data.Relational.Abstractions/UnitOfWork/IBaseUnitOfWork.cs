namespace AM.TestTask.Data.Relational.Abstractions.UnitOfWork;

public interface IBaseUnitOfWork
{
    Task CommitChangesAsync(CancellationToken cancellationToken = default);
}

