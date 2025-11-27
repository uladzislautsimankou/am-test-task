using AM.TestTask.Business.Entities;
using AM.TestTask.Data.Relational.Abstractions.Repositories;

namespace AM.TestTask.Business.Repositories;

public interface IRecClassReadOnlyRepository : IGenericReadonlyRepository<RecClass, int>
{
}
