using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;
using SeerrFixarr.App;

namespace SeerrFixarr.Test;

public static class Extension
{
    public static Movie InCollection(this Movie movie, Collection collection) => movie with { Collection = collection };
    public static Movie WithFile(this Movie movie, MovieFile file) => movie with { HasFile = true, MovieFile = file };

    public static Media ToMediaIssue(this Movie movie) => new()
    {
        Id = movie.Id,
        TmdbId = movie.TmdbId,
        MediaType = MediaType.Movie,
        CreatedAt = movie.Added,
        PlexUrl = "somePlexUrl",
        IosPlexUrl = "someIosPlexUrl",
    };

    public static Media ToMediaIssue(this Episode episode) => new ()
    {
        Id = episode.SeriesId,
        TvdbId = episode.TvdbId,
        MediaType = MediaType.Movie,
        CreatedAt = episode.AirDateUtc,
        PlexUrl = "somePlexUrl",
        IosPlexUrl = "someIosPlexUrl",
    };
    
    public static Issue CreatedBy(this Issue issue, User user,  string comment)
    {
        return issue with
        {
            CreatedBy = user,
            Comments = [
                new Comment
                {
                    User = user,
                    Message = comment,
                }
            ]
        };
    }
    
    public static WebhookIssueRoot ToWebhookIssueRoot(this Issue issue)
    {
        return new WebhookIssueRoot
        {
            IssueId = issue.Id,
            ReportedByUsername = issue.CreatedBy.Username,
        };
    }
}