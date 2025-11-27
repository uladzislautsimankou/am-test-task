using AM.TestTask.Business.Repositories;
using AM.TestTask.Business.Synchronizers;
using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Context;
using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Repositories;
using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Synchronizers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddDataRelationalPostgreSql(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDatabaseContext>(options =>
            options.UseNpgsql(
               configuration.GetConnectionString("ApplicationDatabaseConnection"),
               b => b.MigrationsAssembly(typeof(ApplicationDatabaseContext).Assembly.FullName)));

        services.AddScoped<IMeteoriteReadOnlyRepository, MeteoriteReadOnlyRepository>();
        services.AddScoped<IRecClassReadOnlyRepository, RecClassReadOnlyRepository>();
        services.AddScoped<IMeteoriteTableSychronyzer, MeteoriteTableSychronyzer>();
    }
}
