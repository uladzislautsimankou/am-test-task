namespace AM.TestTask.Business.DTOs;

public record MeteoriteDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Nametype { get; set; } = string.Empty;

    public string Recclass { get; set; } = string.Empty;

    public string Mass { get; set; } = string.Empty;

    public string Fall { get; set; } = string.Empty;

    public string Year { get; set; } = string.Empty;

    public string Reclat { get; set; } = string.Empty;

    public string Reclong { get; set; } = string.Empty;
}
