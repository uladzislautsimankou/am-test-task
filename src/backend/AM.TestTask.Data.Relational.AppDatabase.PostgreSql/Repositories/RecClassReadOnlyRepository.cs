using AM.TestTask.Business.Entities;
using AM.TestTask.Business.Repositories;
using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Context;
using AM.TestTask.Data.Relational.EntityFramework.Repositories;

namespace AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Repositories;

internal sealed class RecClassReadOnlyRepository(ApplicationDatabaseContext context) :
    EntityFrameworkGenericReadonlyRepository<RecClass, int>(context),
    IRecClassReadOnlyRepository
{
}
