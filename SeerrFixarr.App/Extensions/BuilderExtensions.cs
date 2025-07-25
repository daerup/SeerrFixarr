using System.Reflection;
using Meziantou.Framework;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using SeerrFixarr.Api.Lottie;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.KeyProvider;
using SeerrFixarr.App.Lottie;
using SeerrFixarr.App.Runners;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;
using SeerrFixarr.App.Runners.Webhook;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Shared.Settings;
using Serilog;
using Sysinfocus.AspNetCore.Components;
using Log = Serilog.Log;

namespace SeerrFixarr.App.Extensions;

public static class BuilderExtensions
{
    public static void AddDataProtection(this WebApplicationBuilder builder)
    {
        var keyStoragePath = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
            ? "/keys"
            : FullPath.GetFolderPath(Environment.SpecialFolder.ApplicationData) / nameof(SeerrFixarr) / "keys";

        builder.Services
            .AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keyStoragePath))
            .SetApplicationName(nameof(SeerrFixarr));
    }

    public static void AddSettings(this WebApplicationBuilder builder)
    {
        var (optional, reloadOnChange) = (true, true);
        builder.Configuration
            .AddJsonFile("appsettings.json", optional, reloadOnChange)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName.ToLower()}.json", optional, reloadOnChange)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional)
            .AddEnvironmentVariables()
            .Add(new UserRedirectKeyPoolConfigurationSource());
        builder.Services.Configure<SeerrFixarrSettings>(builder.Configuration);
        builder.Services.AddTransient<SeerrFixarrSettings>(sp =>
        {
            var seerrFixarrSettings = sp.GetRequiredService<IOptionsMonitor<SeerrFixarrSettings>>().CurrentValue;
            return seerrFixarrSettings;
        });
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
        services.AddScoped<IssueTargetInformationExtractor>();
        services.AddScoped<WebhookRunner>();
        services.AddScoped<RadarrRunner>();
        services.AddScoped<SonarrRunner>();
        services.AddSingleton<TokenCreator>(serviceProvider =>
        {
            var secret = serviceProvider.GetRequiredService<SeerrFixarrSettings>().JwtSigningKey;
            return new TokenCreator(TimeProvider.System, secret);
        });
        services.AddSingleton<RedirectKeyManager>();
        services.AddTransient<GuidRedirectKeyProvider>();
        services.AddSingleton<FixedRedirectKeyProviderCache>();
        services.AddScoped<RedirectKeyFactory>();
        services.AddScoped<ILottieProvider, LottieProvider>();
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