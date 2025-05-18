using System.Net;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.App.Runners;
using Shouldly;

namespace SeerrFixarr.Test;

public class RadarrIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly FakeOverseerrApi _overseerrApi = new();
    private readonly FakeRadarrApi _radarrApi = new();

    public RadarrIntegrationTests(WebApplicationFactory<Program> application)
    {
        _application = application.WithWebHostBuilder(buiilder =>
        {
            buiilder.ConfigureTestServices(services =>
            {
                services.ConfigureCommon();
                services.AddScoped<IOverseerrApi, FakeOverseerrApi>(_ => _overseerrApi);
                services.AddScoped<IRadarrApi, FakeRadarrApi>(_ => _radarrApi);
            });
        });
    }

    [Theory, CombinatorialData]
    public async Task RedownloadFaultyMovie(
        [CombinatorialValues(null, 9999)] int? idOverride,
        [CombinatorialValues("en", "de", "zh", "")]
        string local)
    {
        // Arrange
        var testUser = TestDataBuilder.TestUser.WithLocale(local);
        var file = TestDataBuilder.CreateMovieFile("some.title.mkv");
        var movie = TestDataBuilder.CreateMovie("Some Title").WithFile(file);
        var issue = TestDataBuilder.CreateIssueFor(GetMovieIdOverride(idOverride, movie)).By(testUser, "Test comment");

        _radarrApi.Setup(movie);
        _overseerrApi.Setup(issue);
        _overseerrApi.Setup(testUser);

        // Act
        var response = await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _overseerrApi.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Id.ShouldBe(issue.Id), i => i.Status.ShouldBe((int)IssueStatus.Resolved)
        );
        _radarrApi.DownloadQueue.ShouldHaveSingleItem().ShouldSatisfyAllConditions(d => d.MovieId.ShouldBe(movie.Id));
        _radarrApi.Movies.ShouldHaveSingleItem().ShouldSatisfyAllConditions(m => m.Id.ShouldBe(movie.Id),
            m => m.MovieFile.ShouldBeNull(), m => m.HasFile.ShouldBeFalse()
        );
        await Verify(_overseerrApi.Comments.Values);
    }

    [Theory, CombinatorialData]
    public async Task DownloadMovie(
        [CombinatorialValues(null, 9999)] int? idOverride,
        [CombinatorialValues("en", "de", "zh", "")]
        string local)
    {
        // Arrange
        var testUser = TestDataBuilder.TestUser.WithLocale(local);
        var movie = TestDataBuilder.CreateMovie("Some Title");
        var issue = TestDataBuilder.CreateIssueFor(GetMovieIdOverride(idOverride, movie)).By(testUser, "Test comment");

        _radarrApi.Setup(movie);
        _overseerrApi.Setup(issue);
        _overseerrApi.Setup(testUser);

        // Act
        var response = await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _overseerrApi.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Id.ShouldBe(issue.Id), i => i.Status.ShouldBe((int)IssueStatus.Resolved)
        );
        _radarrApi.DownloadQueue.ShouldHaveSingleItem().ShouldSatisfyAllConditions(d => d.MovieId.ShouldBe(movie.Id));
        _radarrApi.Movies.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            m => m.Id.ShouldBe(movie.Id), m => m.MovieFile.ShouldBeNull(), m => m.HasFile.ShouldBeFalse()
        );
        await Verify(_overseerrApi.Comments.Values);
    }


    [Theory, CombinatorialData]
    public async Task DownloadAlreadyInProgress(
        [CombinatorialValues(null, 9999)] int? idOverride,
        [CombinatorialValues("en", "de", "zh", "")]
        string local)
    {
        // Arrange
        var testUser = TestDataBuilder.TestUser.WithLocale(local);
        var movie = TestDataBuilder.CreateMovie("Some other Title");
        var issue = TestDataBuilder.CreateIssueFor(GetMovieIdOverride(idOverride, movie)).By(testUser, "Test comment");
        
        _radarrApi.SetupDownloading(movie);
        _overseerrApi.Setup(issue);
        _overseerrApi.Setup(testUser);

        // Act
        var response = await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _overseerrApi.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Id.ShouldBe(issue.Id), i => i.Status.ShouldBe((int)IssueStatus.Resolved)
        );
        _radarrApi.DownloadQueue.ShouldHaveSingleItem().ShouldSatisfyAllConditions(d => d.MovieId.ShouldBe(movie.Id));
        await Verify(_overseerrApi.Comments.Values);
    }


    [Theory, CombinatorialData]
    public async Task TmdbIdFallbackFailureResultsInError([CombinatorialValues("en", "de", "zh", "")] string local)
    {
        // Arrange
        var testUser = TestDataBuilder.TestUser.WithLocale(local);
        var movie = TestDataBuilder.CreateMovie("Some other Title");
        var issue = TestDataBuilder.CreateIssueFor(GetMovieIdOverride(9999, movie)).By(testUser, "Test comment");
        issue = issue with { Media = issue.Media with { TmdbId = null } };

        _radarrApi.SetupDownloading(movie);
        _overseerrApi.Setup(issue);
        _overseerrApi.Setup(testUser);

        // Act
        var response = await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        response.StatusCode.ShouldNotBe(HttpStatusCode.OK);
        await Verify(_overseerrApi.Comments.Values);
    }

    [Theory, CombinatorialData]
    public async Task FailedToGrabMovieFile([CombinatorialValues("en", "de", "zh", "")] string local)
    {
        // Arrange
        var testUser = TestDataBuilder.TestUser.WithLocale(local);
        var file = TestDataBuilder.CreateMovieFile("some.title.mkv");
        var movie = TestDataBuilder.CreateMovie("Some other Title").WithFile(file);
        var issue = TestDataBuilder.CreateIssueFor(movie).By(testUser, "Test comment");
        SetUpCustomAwaitDownloadQueueUpdatedBehavior(() => _radarrApi.DownloadQueue.Clear());

        _overseerrApi.Setup(issue);
        _radarrApi.Setup(movie);
        _overseerrApi.Setup(testUser);

        // Act
        await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        _radarrApi.DownloadQueue.ShouldBeEmpty();
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

    private static Movie GetMovieIdOverride(int? idOverride, Movie movie)
    {
        var movieOverride = movie;
        if (idOverride is not null)
        {
            movieOverride = movieOverride with { Id = idOverride.Value };
        }

        return movieOverride;
    }
}