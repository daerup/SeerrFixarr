using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.KeyProvider;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Shared.Settings;

namespace SeerrFixarr.App.Runners.Webhook;

internal class WebhookRunner(
    CultureScopeFactory scopeFactory,
    IssueTargetInformationExtractor issueTargetInformationExtractor,
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
        var (_, locale) = await overseerr.GetLocalSettingsOfUser(issue.CreatedBy.Id);
        using var translationCulture = scopeFactory.FromLocale(locale);

        await (body.NotificationType switch
        {
            NotificationType.ISSUE_CREATED => HandleIssueCreatedAsync(issue),
            NotificationType.ISSUE_REOPENED => HandleIssueReopenedAsync(issue, locale),
        });
    }

    private async Task HandleIssueCreatedAsync(Issue issue)
    {
        await (issue.Media.MediaType switch
        {
            MediaType.Movie => radarrRunner.HandleMovieIssueAsync(issue),
            MediaType.Tv => sonarrRunner.HandleEpisodeIssueAsync(issue),
        });
    }

    private async Task HandleIssueReopenedAsync(Issue issue, string locale)
    {
        await overseerr.PostIssueComment(issue.Id, Translations.InteractiveInstructions);
        await overseerr.PostIssueComment(issue.Id, Translations.InteractiveSorted);
        var issueInfo = await issueTargetInformationExtractor.ExtractFromAsync(issue);
        var token = tokenCreator.CreateToken(issue.Id, issueInfo, TimeSpan.FromMinutes(10), locale);
        var key = redirectKeyFactory.GetKeyForIdentifier(issue.CreatedBy.PlexUsername);
        redirectKeyManager.AddRedirection(key, token);
        var redirectUrl = $"{settings.ExternalHost}/{key}";
        await overseerr.PostIssueComment(issue.Id, redirectUrl);
    }
}