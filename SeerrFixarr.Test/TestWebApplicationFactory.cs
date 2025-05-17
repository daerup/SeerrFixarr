using System.Text.Json;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;
using SeerrFixarr.App;

namespace SeerrFixarr.Test;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var fakeTimeOutProvider = A.Fake<ITimeOutProvider>();
        A.CallTo(() => fakeTimeOutProvider.AwaitFileDeletion()).Returns(Task.CompletedTask);
        A.CallTo(() => fakeTimeOutProvider.AwaitDownloadQueueUpdated()).Returns(Task.CompletedTask);
        
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IOverseerrApi, FakeOverseerrApi>();
            services.AddSingleton(A.Fake<ISonarrApi>());
            services.AddSingleton<IRadarrApi, FakeRadarrApi>();
            services.AddSingleton(fakeTimeOutProvider);
        });
        builder.UseEnvironment(Environments.Production);
    }

    internal async Task<HttpResponseMessage> CallIssueWebhook(WebhookIssueRoot body)
    {
        using var httpClient = CreateClient();
        var jsonContent =
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("/webhook", jsonContent, TestContext.Current.CancellationToken);
        return response;
    }

    internal (FakeOverseerrApi, ISonarrApi, FakeRadarrApi) GetFakeApis()
    {
        return ((FakeOverseerrApi)Services.GetRequiredService<IOverseerrApi>(),
            Services.GetRequiredService<ISonarrApi>(),
            (FakeRadarrApi)Services.GetRequiredService<IRadarrApi>());
    }
}