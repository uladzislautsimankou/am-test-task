using AM.TestTask.Business.Entities;
using AM.TestTask.Data.Relational.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Context;

internal sealed class ApplicationDatabaseContext(DbContextOptions options) : BaseDbContext(options)
{
    public DbSet<Meteorite> Meteorites => Set<Meteorite>();

    public DbSet<RecClass> RecClasses => Set<RecClass>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");

        base.OnModelCreating(modelBuilder);
    }
}
