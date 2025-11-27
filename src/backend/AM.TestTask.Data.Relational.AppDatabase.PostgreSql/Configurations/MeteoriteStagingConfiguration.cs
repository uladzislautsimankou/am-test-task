using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Configurations;

internal sealed class MeteoriteStagingConfiguration : IEntityTypeConfiguration<MeteoriteStaging>
{
    public void Configure(EntityTypeBuilder<MeteoriteStaging> builder)
    {
        builder.HasNoKey();

        // чтобы EFCore.BulkExtensions мог прочитать метаданные
        builder.ToTable("MeteoriteStaging_Fake", t => t.ExcludeFromMigrations());
    }
}
