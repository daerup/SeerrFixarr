using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SeerrFixarr.Api.Lottie;
using SeerrFixarr.WireMock.Lottie;

namespace SeerrFixarr.WireMock;

public static class BuilderExtensions
{
    private const string HealthEndpoint = "/__admin/health";
    public static async Task<bool> IsRunningInMockedMode(this WebApplicationBuilder _)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri($"http://localhost:{WireMockProgramHealth.Port}");
        client.Timeout = TimeSpan.FromSeconds(2);
        try
        {
            var healthResponse = await client.GetStringAsync(HealthEndpoint);
            return healthResponse.Equals("Healthy", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public static void AddWireMockServices(this IServiceCollection services)
    {
        services.AddScoped<ILottieProvider, WireMockLottieProvider>();
    }
}