using SeerrFixarr.Api.Overseerr;

namespace SeerrFixarr.App;

public class WebhookRunner(IOverseerrApi overseerr, RadarrRunner radarrRunner, SonarrRunner sonarrRunner)
{
    public async Task RunAsync(dynamic body)
    {
        var issue = await overseerr.GetIssue(27);
        var task = issue.Media.MediaType switch
        {
            MediaType.Movie => radarrRunner.HandleMovieIssue(issue),
            MediaType.Tv => sonarrRunner.HandleEpisodeIssue(issue),
            _ => Task.CompletedTask
        };
        
        await task;
    }
}