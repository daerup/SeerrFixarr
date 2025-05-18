using System.Net;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Sonarr;
using SeerrFixarr.App.Runners;
using Shouldly;

namespace SeerrFixarr.Test;

public class SonarrIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly FakeOverseerrApi _overseerrApi = new();
    private readonly FakeSonarrApi _sonarrApi = new();
    
    public SonarrIntegrationTests(WebApplicationFactory<Program> application)
    {
        _application = application.WithWebHostBuilder(buiilder =>
        {
            buiilder.ConfigureTestServices(services =>
            {
                services.ConfigureCommon();
                services.AddScoped<IOverseerrApi, FakeOverseerrApi>(_ => _overseerrApi);
                services.AddScoped<ISonarrApi, FakeSonarrApi>(_ => _sonarrApi);
            });
        });
    }

    [Fact]
    public async Task RedownloadFaultyEpisode()
    {
        // Arrange
        var file = TestDataBuilder.CreateEpisodeFile("some.episode.mkv");
        var episode = TestDataBuilder.CreateEpisode("Some Show", 1).WithFile(file);
        var issue = TestDataBuilder.CreateIssueFor(episode).By(TestDataBuilder.TestUser, "Test comment");

        _sonarrApi.Setup(episode);
        _overseerrApi.Setup(issue);

        // Act
        var response = await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _overseerrApi.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Id.ShouldBe(issue.Id), i => i.Status.ShouldBe((int)IssueStatus.Resolved)
        );
        _sonarrApi.DownloadQueue.ShouldHaveSingleItem().ShouldSatisfyAllConditions(d => d.EpisodeId.ShouldBe(episode.Id));
        _sonarrApi.Episodes.ShouldHaveSingleItem().ShouldSatisfyAllConditions(m => m.Id.ShouldBe(episode.Id),
            m => m.EpisodeFile.ShouldBeNull(), m => m.HasFile.ShouldBeFalse()
        );
        await Verify(_overseerrApi.Comments.Values);
    }
    
    [Fact]
    public async Task DownloadEpisode()
    {
        // Arrange
        var episode = TestDataBuilder.CreateEpisode("Some Show", 1);
        var issue = TestDataBuilder.CreateIssueFor(episode).By(TestDataBuilder.TestUser, "Test comment");

        _sonarrApi.Setup(episode);
        _overseerrApi.Setup(issue);

        // Act
        var response = await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _overseerrApi.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Id.ShouldBe(issue.Id), i => i.Status.ShouldBe((int)IssueStatus.Resolved)
        );
        _sonarrApi.DownloadQueue.ShouldHaveSingleItem().ShouldSatisfyAllConditions(d => d.EpisodeId.ShouldBe(episode.Id));
        _sonarrApi.Episodes.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            m => m.Id.ShouldBe(episode.Id), m => m.EpisodeFile.ShouldBeNull(), m => m.HasFile.ShouldBeFalse()
        );
        await Verify(_overseerrApi.Comments.Values);
    }

    [Fact]
    public async Task DownloadAlreadyInProgress()
    {
        // Arrange
        var episode = TestDataBuilder.CreateEpisode("Some other Show", 1);
        var issue = TestDataBuilder.CreateIssueFor(episode).By(TestDataBuilder.TestUser, "Test comment");
        _sonarrApi.SetupDownloading(episode);
        _overseerrApi.Setup(issue);

        // Act
        var response = await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _overseerrApi.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Id.ShouldBe(issue.Id), i => i.Status.ShouldBe((int)IssueStatus.Resolved)
        );
        _sonarrApi.DownloadQueue.ShouldHaveSingleItem().ShouldSatisfyAllConditions(d => d.EpisodeId.ShouldBe(episode.Id));
        await Verify(_overseerrApi.Comments.Values);
    }
    
    [Fact]
    public async Task FailedToGrabEpisodeFile()
    {
        // Arrange
        var file = TestDataBuilder.CreateEpisodeFile("some.episode.mkv");
        var episode = TestDataBuilder.CreateEpisode("Some other Show", 1).WithFile(file);
        var issue = TestDataBuilder.CreateIssueFor(episode).By(TestDataBuilder.TestUser, "Test comment");
        SetUpCustomAwaitDownloadQueueUpdatedBehavior(() => _sonarrApi.DownloadQueue.Clear());
        _overseerrApi.Setup(issue);
        _sonarrApi.Setup(episode);

        // Act
        await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        _sonarrApi.DownloadQueue.ShouldBeEmpty();
        await Verify(_overseerrApi.Comments.Values);
    }

    private void SetUpCustomAwaitDownloadQueueUpdatedBehavior(Action action)
    {
        var timeOutProvider = _application.Services.GetRequiredService<ITimeOutProvider>();
        A.CallTo(() => timeOutProvider.AwaitDownloadQueueUpdated()).ReturnsLazily(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }
}