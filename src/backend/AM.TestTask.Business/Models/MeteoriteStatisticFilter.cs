namespace AM.TestTask.Business.Models;

public record MeteoriteStatisticFilter
{
    public short? YearFrom { get; init; }

    public short? YearTo { get; init; }

    public int? RecClassId { get; init; }

    public string? NamePart { get; init; }
}