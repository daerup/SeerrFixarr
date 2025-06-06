using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;
using SeerrFixarr.Shared;
using Serilog;

namespace SeerrFixarr.App.Runners.Webhook;

internal class WebhookRunner(
    CultureScopeFactory scopeFactory,
    IOverseerrApi overseerr,
    InteractiveRunner interactiveRunner,
    RadarrRunner radarrRunner,
    SonarrRunner sonarrRunner)
{
    public async Task RunAsync(WebhookIssueRoot body)
    {
        var issue = await overseerr.GetIssue(body.IssueId);
        var (username, locale) = await overseerr.GetLocalSettingsOfUser(issue.CreatedBy.Id);
        using var translationCulture = scopeFactory.FromLocale(username, locale);

        await (body.NotificationType switch
        {
            NotificationType.ISSUE_CREATED => HandleIssueCreated(issue),
            NotificationType.ISSUE_COMMENT => HandleIssueComment(issue),
            NotificationType.ISSUE_REOPENED => HandleIssueReopened(issue),
        });
    }

    private async Task HandleIssueReopened(Issue issue)
    {
        await interactiveRunner.PromptUserForAction(issue);
    }

    private async Task HandleIssueComment(Issue issue)
    {
        var newestComment = issue.Comments.OrderByDescending(c => c.CreatedAt).First();
        var isBotComment = newestComment.Message.First() == Constants.BotIdentificationCharacter; 
        if (isBotComment)
        {
            Log.Debug("Ignoring comment from bot on issue #{IssueId}", issue.Id);
            return;
        }

        await interactiveRunner.FollowUserInstructions(issue, newestComment);
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
}