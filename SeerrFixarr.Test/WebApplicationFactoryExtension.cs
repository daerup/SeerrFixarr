using System.Text.Json;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SeerrFixarr.App;

namespace SeerrFixarr.Test;

public static class WebApplicationFactoryExtension
{
    internal static void ConfigureCommon(this IServiceCollection services)
    {
        var fakeTimeOutProvider = A.Fake<ITimeOutProvider>();
        A.CallTo(() => fakeTimeOutProvider.AwaitFileDeletion()).Returns(Task.CompletedTask);
        A.CallTo(() => fakeTimeOutProvider.AwaitDownloadQueueUpdated()).Returns(Task.CompletedTask);
        services.AddSingleton(fakeTimeOutProvider);
    }

    internal static async Task<HttpResponseMessage> CallIssueWebhook(this WebApplicationFactory<Program> factory, object body)
    {
        using var httpClient = factory.CreateClient();
        var jsonContent =
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("/webhook", jsonContent, TestContext.Current.CancellationToken);
        return response;
    }
}