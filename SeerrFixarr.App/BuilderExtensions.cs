using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SeerrFixarr.Api;
using SeerrFixarr.App.Settings;

namespace SeerrFixarr.App;

public static class BuilderExtensions
{
    public static void AddSeerrFixerrSettings(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("settings/appsettings.json", optional: false, reloadOnChange: true);
        builder.Services.Configure<SeerrFixarrSettings>(builder.Configuration);
    }
    
    public static void AddOverseerrApi(this IServiceCollection services)
    {
        services.AddSingleton<RadarrApi>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SeerrFixarrSettings>>().Value;
            var (apiUrl, apiKey) = options.Radarr;
            return new RadarrApi(sp.GetRequiredService<IHttpClientFactory>(), apiUrl, apiKey);
        });
    }
    
    public static void AddSonarrApi(this IServiceCollection services)
    {
        services.AddSingleton<RadarrApi>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SeerrFixarrSettings>>().Value;
            var (apiUrl, apiKey) = options.Radarr;
            return new RadarrApi(sp.GetRequiredService<IHttpClientFactory>(), apiUrl, apiKey);
        });
    }
    public static void AddRadarrApi(this IServiceCollection services)
    {
        services.AddSingleton<RadarrApi>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SeerrFixarrSettings>>().Value;
            var (apiUrl, apiKey) = options.Radarr;
            return new RadarrApi(sp.GetRequiredService<IHttpClientFactory>(), apiUrl, apiKey);
        });
    }
    
}