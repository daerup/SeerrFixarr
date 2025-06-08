using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.KeyProvider;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Shared.Settings;

namespace SeerrFixarr.App.Runners.Webhook;

internal class WebhookRunner(
    CultureScopeFactory scopeFactory,
    IOverseerrApi overseerr,
    RadarrRunner radarrRunner,
    SonarrRunner sonarrRunner,
    TokenCreator tokenCreator,
    RedirectKeyManager redirectKeyManager,
    RedirectKeyFactory redirectKeyFactory,
    SeerrFixarrSettings settings)
{
    public async Task RunAsync(WebhookIssueRoot body)
    {
        var issue = await overseerr.GetIssue(body.IssueId);
        var (username, locale) = await overseerr.GetLocalSettingsOfUser(issue.CreatedBy.Id);
        using var translationCulture = scopeFactory.FromLocale(username, locale);

        await (body.NotificationType switch
        {
            NotificationType.ISSUE_CREATED => HandleIssueCreated(issue),
            NotificationType.ISSUE_REOPENED => HandleIssueReopened(issue),
        });
    }

    private async Task HandleIssueCreated(Issue issue)
    {
        await (issue.Media.MediaType switch
        {
            MediaType.Movie => radarrRunner.HandleMovieIssue(issue),
            MediaType.Tv => sonarrRunner.HandleEpisodeIssue(issue),
            _ => Task.CompletedTask
        });
    }

    private async Task HandleIssueReopened(Issue issue)
    {
        var token = tokenCreator.CreateToken(issue.Media.Id, issue.Media.MediaType, TimeSpan.FromMinutes(10));
        var key = redirectKeyFactory.GetKeyForIdentifier(issue.CreatedBy.Username);
        redirectKeyManager.AddRedirection(key, token);
        var redirectUrl = $"{settings.ExternalHost}/{key}";
        await overseerr.PostIssueComment(issue.Id, redirectUrl);
    }
}