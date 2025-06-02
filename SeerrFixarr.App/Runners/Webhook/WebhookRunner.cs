using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;

namespace SeerrFixarr.App.Runners.Webhook;

internal class WebhookRunner(CultureScopeFactory scopeFactory, IOverseerrApi overseerr, RadarrRunner radarrRunner, SonarrRunner sonarrRunner)
{
    public async Task RunAsync(WebhookIssueRoot body)
    {
        var issue = await overseerr.GetIssue(body.IssueId);
        var (username, locale) = await overseerr.GetLocalSettingsOfUser(issue.CreatedBy.Id);

        using var culture = scopeFactory.FromLocale(username, locale);
        await (issue.Media.MediaType switch
        {
            MediaType.Movie => radarrRunner.HandleMovieIssue(issue),
            MediaType.Tv => sonarrRunner.HandleEpisodeIssue(issue),
            _ => Task.CompletedTask
        });
    }
}