using Microsoft.Extensions.Time.Testing;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;
using UnitsNet;

namespace SeerrFixarr.Test.Infrastructure;

internal static class TestDataBuilder
{
    public static readonly TimeProvider FakeTimeProvider =
        new FakeTimeProvider(new DateTimeOffset(2025, 12, 31, 12, 05, 57, TimeSpan.Zero));

    private static int idSequence;

    public static readonly User TestUser;

    static TestDataBuilder()
    {
        var id = GetNextId();
        TestUser = new User
        {
            Id = id,
            Username = "TestUser",
            Email = "test@user.com",
            PlexUsername = "TestPlexUser",
            PlexId = 123456 + id,
            CreatedAt = FakeTimeProvider.GetUtcNow().DateTime,
            RequestCount = 5,
            DisplayName = "Test User"
        };
    }

    public static void Reset() => idSequence = 0;

    public static Issue CreateIssueFor(Movie movie)
    {
        var id = GetNextId();
        var media = movie.ToMediaIssue();
        return new Issue
        {
            Id = id,
            Status = (int)IssueStatus.Open,
            CreatedAt = FakeTimeProvider.GetLocalNow().Date,
            UpdatedAt = FakeTimeProvider.GetLocalNow().Date,
            ProblemEpisode = 0,
            ProblemSeason = 0,
            Media = media,
        };
    }
    
    public static Issue CreateIssueFor(Episode episode)
    {
        var id = GetNextId();
        var media = episode.ToMediaIssue();
        return new Issue
        {
            Id = id,
            Status = (int)IssueStatus.Open,
            CreatedAt = FakeTimeProvider.GetLocalNow().Date,
            UpdatedAt = FakeTimeProvider.GetLocalNow().Date,
            ProblemEpisode = episode.EpisodeNumber,
            ProblemSeason = episode.SeasonNumber,
            Media = media,
        };
    }

    public static Movie CreateMovie(string title)
    {
        var id = GetNextId();
        return new Movie
        {
            Id = id,
            Title = title,
            TmdbId = 123456 + id,
            Added = FakeTimeProvider.GetUtcNow().DateTime,
            HasFile = false,
            Monitored = true,
            ReleaseDate = FakeTimeProvider.GetUtcNow().DateTime.Subtract(TimeSpan.FromDays(100)),
        };
    }

    public static MovieFile CreateMovieFile(string title)
    {
      var id = GetNextId();
      return new MovieFile
      {
        Id = id,
        Path = title,
        Size = (long)Information.FromGibibytes(5).Bytes
      };
    }
    
    public static Episode CreateEpisode(string seriesTitle, int episodeNumber)
    {
      var id = GetNextId();
      var seriesId = seriesTitle.ToCharArray().Select(c => (int)c).Sum();
      return new Episode
      {
        Id = id,
        SeriesId = seriesId,
        SeasonNumber = 1,
        EpisodeNumber = episodeNumber,
        Title = "Test Episode",
        TvdbId = 123456 + seriesId,
        AirDateUtc = FakeTimeProvider.GetUtcNow().DateTime.Subtract(TimeSpan.FromDays(100)),
        HasFile = false,
        Monitored = true
      };
    }

    public static EpisodeFile CreateEpisodeFile(string title)
    {
      var id = GetNextId();
      return new EpisodeFile
      {
        Id = id,
        Path = title,
        Size = (long)Information.FromGibibytes(5).Bytes
      };
    }

    private static int GetNextId()
    {
        return idSequence++;
    }
}