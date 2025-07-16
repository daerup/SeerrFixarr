using System.Text.Json;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;
using SeerrFixarr.App.Runners;

namespace SeerrFixarr.Test.Infrastructure;

public static class WebApplicationFactoryExtension
{
    internal static void ConfigureCommon(this IServiceCollection services)
    {
        var fakeTimeOutProvider = A.Fake<ITimeOutProvider>();
        A.CallTo(() => fakeTimeOutProvider.AwaitFileDeletionAsync()).Returns(Task.CompletedTask);
        A.CallTo(() => fakeTimeOutProvider.AwaitDownloadQueueUpdatedAsync()).Returns(Task.CompletedTask);
        services.AddSingleton(fakeTimeOutProvider);
        services.AddScoped<IRadarrApi>(_ => A.Fake<IRadarrApi>());
        services.AddScoped<ISonarrApi>(_ => A.Fake<ISonarrApi>());
        services.AddScoped<IOverseerrApi>(_ => A.Fake<IOverseerrApi>());
    }

    internal static async Task<HttpResponseMessage> CallIssueWebhookAsync(this WebApplicationFactory<Program> factory, object body)
    {
        using var httpClient = factory.CreateClient();
        var jsonContent =
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("/webhook", jsonContent, TestContext.Current.CancellationToken);
        return response;
    }
}