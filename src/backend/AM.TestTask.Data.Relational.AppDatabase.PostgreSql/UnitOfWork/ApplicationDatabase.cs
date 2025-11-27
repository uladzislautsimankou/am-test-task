using AM.TestTask.Business.UnitOfWork;
using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Context;
using AM.TestTask.Data.Relational.EntityFramework.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace AM.TestTask.Data.Relational.AppDatabase.PostgreSql.UnitOfWork;

internal sealed class ApplicationDatabase(
    ApplicationDatabaseContext context,
    ILogger<ApplicationDatabase> logger
    ) : BaseUnitOfWork(context, logger), IApplicationDatabase
{
}
