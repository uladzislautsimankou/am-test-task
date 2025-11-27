using AM.TestTask.Business.Enums;
using AM.TestTask.Data.Relational.Abstractions;

namespace AM.TestTask.Business.Entities;

public class Meteorite : IIdentifiablyEntity<long>
{
    public long Id { get; set; } // подозреваем, что в json уникальный айди, который можно использовать и как наш айдишник
    public string Name { get; set; } = string.Empty;

    public int RecClassId { get; set; }
    public virtual RecClass RecClass { get; set; } = new();

    public NameType NameType { get; set; }
    public FallType FallType { get; set; }

    public decimal? Mass { get; set; }
    public short? Year { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
