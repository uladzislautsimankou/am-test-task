namespace AM.TestTask.Business.DTOs;

public record MeteoriteStatisticDto
{
    public short? Year { get; set; }

    public int Count { get; set; }

    public decimal TotalMass { get; set; }
}
