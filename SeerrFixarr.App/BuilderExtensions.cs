using System.Reflection;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.Runners;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;
using SeerrFixarr.App.Runners.Webhook;
using SeerrFixarr.Shared.Settings;
using Serilog;

namespace SeerrFixarr.App;

public static class BuilderExtensions
{
    public static void AddLogging(this WebApplicationBuilder builder)
    {
        builder.Services.AddSerilog(c =>
            c.ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().WriteTo.Console());
    }

    public static void AddSettings(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
        builder.Services.Configure<SeerrFixarrSettings>(builder.Configuration);
    }

    public static void AddSeerrFixarrServices(this IServiceCollection services)
    {
        services.Decorate<IOverseerrApi, OverseerrApiLoggingInterceptor>();
        services.AddScoped<CultureScopeFactory>();
        services.AddScoped<ITimeOutProvider, TimeOutProvider>();
        services.AddScoped<WebhookRunner>();
        services.AddScoped<RadarrRunner>();
        services.AddScoped<SonarrRunner>();
    }
}