using Microsoft.Extensions.Time.Testing;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using UnitsNet;

namespace SeerrFixarr.Test;

internal static class TestDataBuilder
{
    public static TimeProvider FakeTimeProvider =
        new FakeTimeProvider(new DateTimeOffset(2025, 12, 31, 12, 05, 57, TimeSpan.Zero));

    private static int _idSequence = 0;

    public static User TestUser;

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

    public static void Reset() => _idSequence = 0;

    public static Issue CreateIssueFor(MediaUnion mediaUnion)
    {
        var id = GetNextId();
        var media = mediaUnion.Match(
            issue => issue.Movie.ToMediaIssue(),
            issue => issue.Episode.ToMediaIssue()
        );

        var (problemSesons, problemEpisode) = mediaUnion.Match(
            _ => (0, 0),
            episode => (episode.Episode.SeasonNumber, episode.Episode.EpisodeNumber)
        );

        return new Issue
        {
            Id = id,
            Status = (int)IssueStatus.Open,
            CreatedAt = FakeTimeProvider.GetLocalNow().Date,
            UpdatedAt = FakeTimeProvider.GetLocalNow().Date,
            ProblemEpisode = problemEpisode,
            ProblemSeason = problemSesons,
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

    private static int GetNextId()
    {
        return _idSequence++;
    }
}