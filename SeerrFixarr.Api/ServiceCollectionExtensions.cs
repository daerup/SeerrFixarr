using Microsoft.Extensions.DependencyInjection;
using Refit;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;
using SeerrFixarr.Shared.Settings;

namespace SeerrFixarr.Api;

public static class ServiceCollectionExtensions
{
    public static void AddArrApis(this IServiceCollection services)
    {
        services
            .AddRefitClient<IRadarrApi>()
            .ConfigureApiClient(settings => settings.Radarr);
        services
            .AddRefitClient<ISonarrApi>()
            .ConfigureApiClient(settings => settings.Sonarr);
        services
            .AddRefitClient<IOverseerrApi>()
            .ConfigureApiClient(settings => settings.Overseerr);
    }
    
    private static void ConfigureApiClient(this IHttpClientBuilder builder, Func<SeerrFixarrSettings, ApiSettings> configSelector)
    {
        builder.ConfigureHttpClient((serviceProvider, client) =>
        {
            var (apiUrl, apiKey) = configSelector(serviceProvider.GetRequiredService<SeerrFixarrSettings>());
            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            client.BaseAddress = new Uri(apiUrl);
        });
    }
}