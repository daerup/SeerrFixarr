using System.Net;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting.Internal;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;
using Shouldly;

namespace SeerrFixarr.Test;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly FakeOverseerrApi _overseerrApi = new();
    private readonly FakeRadarrApi _radarrApi = new();
    private readonly ISonarrApi _sonarrApi = A.Fake<ISonarrApi>();
    
    public IntegrationTests(WebApplicationFactory<Program> application)
    {
        _application = application.WithWebHostBuilder(buiilder =>
        {
            buiilder.ConfigureTestServices(services =>
            {
                services.ConfigureCommon();
                services.AddScoped<IOverseerrApi, FakeOverseerrApi>(_ => _overseerrApi);
                services.AddScoped<ISonarrApi>(_ => _sonarrApi);
                services.AddScoped<IRadarrApi, FakeRadarrApi>(_ => _radarrApi);
            });
        });
        TestDataBuilder.Reset();
    }

    [Fact]
    public async Task RedownloadFaultyMovie()
    {
        // Arrange
        var file = TestDataBuilder.CreateMovieFile("some.title.mkv");
        var movie = TestDataBuilder.CreateMovie("Some Title").WithFile(file);
        var issue = TestDataBuilder.CreateIssueFor(movie).CreatedBy(TestDataBuilder.TestUser, "Test comment");

        _radarrApi.Setup(movie);
        _overseerrApi.Setup(issue);

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
        await Verify(_overseerrApi.Comments);
    }

    [Fact]
    public async Task DownloadMovie()
    {
        // Arrange
        
        var movie = TestDataBuilder.CreateMovie("Some Title");
        var issue = TestDataBuilder.CreateIssueFor(movie).CreatedBy(TestDataBuilder.TestUser, "Test comment");

        _radarrApi.Setup(movie);
        _overseerrApi.Setup(issue);

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
        await Verify(_overseerrApi.Comments);
    }

    [Fact]
    public async Task DownloadAlreadyInProgress()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie("Some other Title");
        var issue = TestDataBuilder.CreateIssueFor(movie).CreatedBy(TestDataBuilder.TestUser, "Test comment");

        _radarrApi.SetupDownloading(movie);
        _overseerrApi.Setup(issue);

        // Act
        var response = await _application.CallIssueWebhook(issue.ToWebhookIssueRoot());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _overseerrApi.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Id.ShouldBe(issue.Id), i => i.Status.ShouldBe((int)IssueStatus.Resolved)
        );
        _radarrApi.DownloadQueue.ShouldHaveSingleItem().ShouldSatisfyAllConditions(d => d.MovieId.ShouldBe(movie.Id));
        await Verify(_overseerrApi.Comments);
    }
}