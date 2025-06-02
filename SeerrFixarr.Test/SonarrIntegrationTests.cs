using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Sonarr;
using Shouldly;
using Xunit;

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

    [Theory, CombinatorialData]
    public async Task RedownloadFaultyEpisode([CombinatorialValues("en", "de", "zh", "")] string local)
    {
        // Arrange
        var testUser = TestDataBuilder.TestUser.WithLocale(local);
        var file = TestDataBuilder.CreateEpisodeFile("some.episode.mkv");
        var episode = TestDataBuilder.CreateEpisode("Some Show", 1).WithFile(file);
        var issue = TestDataBuilder.CreateIssueFor(episode).By(testUser, "Test comment");

        _sonarrApi.Setup(episode);
        _overseerrApi.Setup(issue);
        _overseerrApi.Setup(testUser);

        // Act
        var response = await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _overseerrApi.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Id.ShouldBe(issue.Id), i => i.Status.ShouldBe((int)IssueStatus.Resolved)
        );
        _sonarrApi.DownloadQueue.ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(d => d.EpisodeId.ShouldBe(episode.Id));
        _sonarrApi.Episodes.ShouldHaveSingleItem().ShouldSatisfyAllConditions(m => m.Id.ShouldBe(episode.Id),
            m => m.EpisodeFile.ShouldBeNull(), m => m.HasFile.ShouldBeFalse()
        );
        await Verify(_overseerrApi.Comments.Values);
    }

    [Theory, CombinatorialData]
    public async Task DownloadEpisode([CombinatorialValues("en", "de", "zh", "")] string local)
    {
        // Arrange
        var testUser = TestDataBuilder.TestUser.WithLocale(local);
        var episode = TestDataBuilder.CreateEpisode("Some Show", 1);
        var issue = TestDataBuilder.CreateIssueFor(episode).By(testUser, "Test comment");

        _sonarrApi.Setup(episode);
        _overseerrApi.Setup(issue);
        _overseerrApi.Setup(testUser);

        // Act
        var response = await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _overseerrApi.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Id.ShouldBe(issue.Id), i => i.Status.ShouldBe((int)IssueStatus.Resolved)
        );
        _sonarrApi.DownloadQueue.ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(d => d.EpisodeId.ShouldBe(episode.Id));
        _sonarrApi.Episodes.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            m => m.Id.ShouldBe(episode.Id), m => m.EpisodeFile.ShouldBeNull(), m => m.HasFile.ShouldBeFalse()
        );
        await Verify(_overseerrApi.Comments.Values);
    }

    [Theory, CombinatorialData]
    public async Task DownloadAlreadyInProgress([CombinatorialValues("en", "de", "zh", "")] string local)
    {
        // Arrange
        var testUser = TestDataBuilder.TestUser.WithLocale(local);
        var episode = TestDataBuilder.CreateEpisode("Some other Show", 1);
        var issue = TestDataBuilder.CreateIssueFor(episode).By(testUser, "Test comment");

        _sonarrApi.SetupDownloading(episode);
        _overseerrApi.Setup(issue);
        _overseerrApi.Setup(testUser);

        // Act
        var response = await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _overseerrApi.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Id.ShouldBe(issue.Id), i => i.Status.ShouldBe((int)IssueStatus.Resolved)
        );
        _sonarrApi.DownloadQueue.ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(d => d.EpisodeId.ShouldBe(episode.Id));
        await Verify(_overseerrApi.Comments.Values);
    }

    [Theory, CombinatorialData]
    public async Task FailedToGrabEpisodeFile([CombinatorialValues("en", "de", "zh", "")] string local)
    {
        // Arrange
        var testUser = TestDataBuilder.TestUser.WithLocale(local);
        var file = TestDataBuilder.CreateEpisodeFile("some.episode.mkv");
        var episode = TestDataBuilder.CreateEpisode("Some other Show", 1).WithFile(file);
        var issue = TestDataBuilder.CreateIssueFor(episode).By(testUser, "Test comment");
        TestHelper.SetUpCustomAwaitDownloadQueueUpdatedBehavior(_application, () => _sonarrApi.DownloadQueue.Clear());

        _overseerrApi.Setup(issue);
        _sonarrApi.Setup(episode);
        _overseerrApi.Setup(testUser);

        // Act
        await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        _sonarrApi.DownloadQueue.ShouldBeEmpty();
        await Verify(_overseerrApi.Comments.Values);
    }

    [Theory, CombinatorialData]
    public async Task FailedToGrabEpisodeFileRetries([CombinatorialValues("en", "de", "zh", "")] string local)
    {
        // Arrange
        var testUser = TestDataBuilder.TestUser.WithLocale(local);
        var file = TestDataBuilder.CreateEpisodeFile("some.episode.mkv");
        var episode = TestDataBuilder.CreateEpisode("Some other Show", 1).WithFile(file);
        var issue = TestDataBuilder.CreateIssueFor(episode).By(testUser, "Test comment");

        List<EpisodeDownload> queue = [];
        TestHelper.SetUpCustomAwaitDownloadQueueUpdatedBehavior(_application, () =>
        {
            queue.AddRange(_sonarrApi.DownloadQueue);
            _sonarrApi.DownloadQueue.Clear();
        }, () => _sonarrApi.DownloadQueue.AddRange(queue));

        _overseerrApi.Setup(issue);
        _sonarrApi.Setup(episode);
        _overseerrApi.Setup(testUser);

        // Act
        await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        _sonarrApi.DownloadQueue.Count.ShouldBe(1);
        await Verify(_overseerrApi.Comments.Values);
    }
}