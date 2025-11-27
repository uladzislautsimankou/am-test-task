namespace AM.TestTask.Data.Cache.Options;

internal record RedisOptions
{
    public string Configuration { get; set; } = "localhost";

    public string InstanceName { get; set; } = "MeteoriteApp_";
}
