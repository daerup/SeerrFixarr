using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.Runners;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;
using SeerrFixarr.App.Runners.Webhook;
using SeerrFixarr.Shared.Settings;
using Serilog;
using Sysinfocus.AspNetCore.Components;
using Log = Serilog.Log;

namespace SeerrFixarr.App.Extensions;

public static class BuilderExtensions
{
    public static void AddDataProtection(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddDataProtection()
            .UseEphemeralDataProtectionProvider()
            .SetApplicationName(nameof(SeerrFixarr));
    }

    public static void AddSettings(this WebApplicationBuilder builder)
    {
        var (optional, reloadOnChange) = (true, true);
        builder.Configuration
            .AddJsonFile("appsettings.json", optional, reloadOnChange)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName.ToLower()}.json", optional, reloadOnChange)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional);
        builder.Services.Configure<SeerrFixarrSettings>(builder.Configuration);
    }

    public static void AddLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console().CreateLogger();
        builder.Services.AddSerilog(Log.Logger);
    }

    public static void AddSeerrFixarrServices(this IServiceCollection services)
    {
        services.Decorate<IOverseerrApi, OverseerrApiLoggingInterceptor>();
        services.AddScoped<CultureScopeFactory>();
        services.AddScoped<ITimeOutProvider, TimeOutProvider>();
        services.AddScoped<WebhookRunner>();
        services.AddScoped<RadarrRunner>();
        services.AddScoped<SonarrRunner>();
        services.AddSingleton<TokenCreator>(serviceProvider =>
        {
            var secret = serviceProvider.GetRequiredService<IOptions<SeerrFixarrSettings>>().Value.JwtSigningKey;
            return new TokenCreator(secret);
        });
    }

    public static void AddBlazor(this IServiceCollection services)
    {
        services.AddSysinfocus(jsCssFromCDN: false);
        services.AddAntiforgery(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });
        services.AddRazorComponents().AddInteractiveServerComponents();
    }
}