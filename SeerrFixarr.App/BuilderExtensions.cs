using System.Reflection;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.Runners;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;
using SeerrFixarr.App.Runners.Webhook;
using SeerrFixarr.Shared.Settings;

namespace SeerrFixarr.App;

public static class BuilderExtensions
{
    public static void AddSeerrFixerrSettings(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
        builder.Services.Configure<SeerrFixarrSettings>(builder.Configuration);
    }
    
    public static void RegisterServices(this IServiceCollection services)
    {
        services.Decorate<IOverseerrApi, OverseerrApiLoggingInterceptor>();
        services.AddScoped<CultureScopeFactory>();
        services.AddScoped<ITimeOutProvider, TimeOutProvider>();
        services.AddScoped<WebhookRunner>();
        services.AddScoped<RadarrRunner>();
        services.AddScoped<SonarrRunner>();
    }
}