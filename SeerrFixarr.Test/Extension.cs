using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;
using SeerrFixarr.App.Runners.Webhook;

namespace SeerrFixarr.Test;

internal record UserWithSettings(User User, UserLocalSettings Settings)
{
    public static implicit operator User(UserWithSettings user) => user.User;
}

internal static class Extension
{
    public static Movie InCollection(this Movie movie, Collection collection) => movie with { Collection = collection };
    public static Movie WithFile(this Movie movie, MovieFile file) => movie with { HasFile = true, MovieFile = file };
    public static Episode WithFile(this Episode episode, EpisodeFile file) => episode with { HasFile = true, EpisodeFile = file };

    public static UserWithSettings WithLocale(this User user, string locale) => new UserWithSettings(user,
      new UserLocalSettings
      {
        Username = user.DisplayName,
        Locale = locale,
      });

    public static Media ToMediaIssue(this Movie movie) => new()
    {
        Id = movie.Id,
        TmdbId = movie.TmdbId,
        MediaType = MediaType.Movie,
        CreatedAt = movie.Added,
        PlexUrl = "somePlexUrl",
        IosPlexUrl = "someIosPlexUrl",
    };

    public static Media ToMediaIssue(this Episode episode) => new()
    {
        Id = episode.SeriesId,
        TvdbId = episode.TvdbId,
        MediaType = MediaType.Tv,
        CreatedAt = episode.AirDateUtc,
        PlexUrl = "somePlexUrl",
        IosPlexUrl = "someIosPlexUrl",
    };

    public static Issue By(this Issue issue, User user, string comment)
    {
        return issue with
        {
            CreatedBy = user,
            Comments =
            [
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

    public static T SingleWithApiException<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        try
        {
            return collection.Single(predicate);
        }
        catch (Exception e)
        {
            throw new FakeApiException(e);
        }
    }

    public static T? SingleOrDefaultWithApiException<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        try
        {
            return collection.SingleOrDefault(predicate);
        }
        catch (Exception e)
        {
            throw new FakeApiException(e);
        }
    }
}