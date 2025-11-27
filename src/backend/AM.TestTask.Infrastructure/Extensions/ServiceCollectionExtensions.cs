using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Services.Interfaces;
using AM.TestTask.Infrastructure.Jobs;
using AM.TestTask.Infrastructure.Options;
using AM.TestTask.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace AM.TestTask.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MeteoriteDataProviderOptions>(configuration.GetSection("MeteoriteDataProvider"));
        services.AddHttpClient();
        services.AddScoped<IDataProvider<MeteoriteDto>, MeteoriteHttpDataProvider>();

        var syncCron = configuration.GetValue<string>("MeteoriteSyncCronSchedule") ?? "0 0 * * * ?";

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("MeteoriteSyncJob");
            q.AddJob<MeteoriteSyncJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("MeteoriteSyncTrigger")
                .WithCronSchedule(syncCron));
        });

        services.AddQuartzHostedService(q =>
        {
            q.WaitForJobsToComplete = true;
        });
    }
}
