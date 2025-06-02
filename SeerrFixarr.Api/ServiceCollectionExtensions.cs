using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            .ConfigureApiClient<SeerrFixarrSettings>(settings => settings.Radarr);
        services
            .AddRefitClient<ISonarrApi>()
            .ConfigureApiClient<SeerrFixarrSettings>(settings => settings.Sonarr);
        services
            .AddRefitClient<IOverseerrApi>()
            .ConfigureApiClient<SeerrFixarrSettings>(settings => settings.Overseerr);
    }
    
    private static void ConfigureApiClient<TSettings>(this IHttpClientBuilder builder, Func<TSettings, ApiSettings> configSelector) where TSettings : class, new()
    {
        builder.ConfigureHttpClient((serviceProvider, client) =>
        {
            var (apiUrl, apiKey) = configSelector(serviceProvider.GetRequiredService<IOptions<TSettings>>().Value);
            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            client.BaseAddress = new Uri(apiUrl);
        });
    }
}