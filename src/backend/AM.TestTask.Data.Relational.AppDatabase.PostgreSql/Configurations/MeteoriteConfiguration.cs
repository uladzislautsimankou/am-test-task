using AM.TestTask.Business.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Configurations;

internal sealed class MeteoriteConfiguration : IEntityTypeConfiguration<Meteorite>
{
    public void Configure(EntityTypeBuilder<Meteorite> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.HasIndex(m => new { m.Year, m.RecClassId })
            .IncludeProperties(m => m.Mass);

        builder.HasIndex(m => m.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
