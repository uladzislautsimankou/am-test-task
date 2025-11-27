using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Enums;
using System.Globalization;

namespace AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Models;

internal sealed class MeteoriteStaging
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string RecClassRaw { get; set; } = string.Empty;

    public NameType NameType { get; set; }
    public FallType FallType { get; set; }

    public decimal? Mass { get; set; }
    public short? Year { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}

