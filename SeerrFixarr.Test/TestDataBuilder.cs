using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using UnitsNet;

namespace SeerrFixarr.Test;

internal static class TestDataBuilder
{
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
            PlexId = 123456+id,
            CreatedAt = DateTime.Now,
            RequestCount = new Random().Next(0, 10),
            DisplayName = "Test User"
        };

    }

    public static Issue CreateIssueFor(MediaUnion mediaUnion)
    {
        var id = GetNextId();
        var media = mediaUnion.Match(
            issue => issue.Movie.ToMediaIssue(),
            issue => issue.Episode.ToMediaIssue()
        );
        
        var (problemSesons, problemEpisode)  = mediaUnion.Match(
            _ => (0,0),
            episode => (episode.Episode.SeasonNumber, episode.Episode.EpisodeNumber)
        );
        
        return new Issue
        {
            Id = id,
            Status = (int)IssueStatus.Open,
            CreatedAt = DateTime.Today,
            UpdatedAt = DateTime.Today,
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
            TmdbId = 123456+id,
            Added = DateTime.Now,
            HasFile = false,
            Monitored = true,
            ReleaseDate = DateTime.Now.Subtract(TimeSpan.FromDays(100)),
        };
    }

    public static MovieFile CreateMovieFile(string title)
    {
        var id = GetNextId();
        return new MovieFile
        {
            Id = id,
            Path = title,
            Size = GetRandomSize(),
        };
    }
    
    private static int GetNextId()
    {
        return _idSequence++;
    }
    
    private static long GetRandomSize()
    {
        var random = new Random();
        var gigabytes = random.Next(1, 30);
        return (long)Information.FromGibibytes(gigabytes).Bytes;
    }
}