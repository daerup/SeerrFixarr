using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;

namespace SeerrFixarr.App.Runners.Webhook;

public class WebhookRunner(IOverseerrApi overseerr, RadarrRunner radarrRunner, SonarrRunner sonarrRunner)
{
    public async Task RunAsync(WebhookIssueRoot body)
    {
        var issue = await overseerr.GetIssue(body.IssueId);
        var task = issue.Media.MediaType switch
        {
            MediaType.Movie => radarrRunner.HandleMovieIssue(issue),
            MediaType.Tv => sonarrRunner.HandleEpisodeIssue(issue),
            _ => Task.CompletedTask
        };
        
        await task;
    }
}