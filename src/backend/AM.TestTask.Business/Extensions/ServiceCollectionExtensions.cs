using AM.TestTask.Business.Services;
using AM.TestTask.Business.Services.Interfaces;
using AM.TestTask.Business.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AM.TestTask.Business.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddBusiness(this IServiceCollection services)
    {
        services.AddScoped<IMeteoriteSyncService, MeteoriteSyncService>();
        services.AddScoped<IMeteoriteReadService, MeteoriteReadService>();

        services.AddValidatorsFromAssemblyContaining<MeteoriteStatisticFilterValidator>();
    }
}
