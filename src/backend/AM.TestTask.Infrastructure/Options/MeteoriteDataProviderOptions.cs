namespace AM.TestTask.Infrastructure.Options;

internal record MeteoriteDataProviderOptions
{
    public string Url { get; set; } = string.Empty;
}