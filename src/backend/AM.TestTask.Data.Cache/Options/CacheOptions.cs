namespace AM.TestTask.Data.Cache.Options;

internal record CacheOptions
{
    public string Provider { get; set; } = "InMemory";

    public int TtlMinutes { get; set; } = 15; // только пока никакого другого кэша со соим ттл нет

    public RedisOptions? Redis { get; set; }
}
