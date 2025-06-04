using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SeerrFixarr.App.Runners;

namespace SeerrFixarr.Test;

internal static class TestHelper
{
    public static void SetUpCustomAwaitDownloadQueueUpdatedBehavior(WebApplicationFactory<Program> application,
        params Action[] action)
    {
        var timeOutProvider = application.Services.GetRequiredService<ITimeOutProvider>();
        var callCount = 0;
        A.CallTo(() => timeOutProvider.AwaitDownloadQueueUpdated()).ReturnsLazily(() =>
        {
            var index = callCount % action.Length;
            action[index]();
            callCount++;
            return Task.CompletedTask;
        });
    }
}