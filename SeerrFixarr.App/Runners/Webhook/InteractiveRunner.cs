using CSharpFunctionalExtensions;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.KeyProvider;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Shared.Settings;

namespace SeerrFixarr.App.Runners.Webhook;

internal class InteractiveRunner(
    IOverseerrApi overseerr,
    RadarrRunner radarrRunner,
    SonarrRunner sonarrRunner,
    TokenCreator tokenCreator,
    RedirectKeyManager redirectKeyManager,
    RedirectKeyFactory redirectKeyFactory,
    SeerrFixarrSettings settings)
{
    private readonly List<(UserActions action, string message)> _allowedUserActions =
    [
        (UserActions.AutomaticallyGrab, Translations.HowToProceedOption1),
        (UserActions.InteractiveGrab, Translations.HowToProceedOption2)
    ];

    public async Task PromptUserForAction(Issue issue)
    {
        await overseerr.PostIssueComment(issue.Id, Translations.HowToProceed);
        foreach (var (_, message) in _allowedUserActions)
        {
            await overseerr.PostIssueComment(issue.Id, message);
        }
    }

    public async Task FollowUserInstructions(Issue issue, Comment comment)
    {
        var choosenAction = int.TryParse(comment.Message, out var result) ? Maybe<int>.From(result) : Maybe<int>.None;

        await choosenAction.ExecuteNoValue(() => overseerr.PostIssueComment(issue.Id, Translations.HowToProceed));
        await choosenAction.Execute(choice => FollowUserInstructionsInternal(choice, issue, comment));
    }

    private async Task FollowUserInstructionsInternal(int choice, Issue issue, Comment comment)
    {
        try
        {
            var choosenAction = _allowedUserActions[choice - 1].action;
            await (choosenAction switch
            {
                UserActions.AutomaticallyGrab => AutomaticallyGrab(issue),
                UserActions.InteractiveGrab => InteractivelyGrab(issue, comment),
            });
        }
        catch (ArgumentOutOfRangeException)
        {
            await overseerr.PostIssueComment(issue.Id, Translations.HowToProceedOptionOutOfRange);
        }
    }

    private async Task AutomaticallyGrab(Issue issue)
    {
        await (issue.Media.MediaType switch
        {
            MediaType.Movie => radarrRunner.HandleMovieIssue(issue),
            MediaType.Tv => sonarrRunner.HandleEpisodeIssue(issue),
            _ => Task.CompletedTask
        });
    }

    private async Task InteractivelyGrab(Issue issue, Comment comment)
    {
        var token = tokenCreator.CreateToken(issue.Id, issue.Media.MediaType, TimeSpan.FromMinutes(10));
        var key = redirectKeyFactory.GetKeyForIdentifier(comment.User.Username);
        redirectKeyManager.AddRedirection(key, token);
        var redirectUrl = $"{settings.ExternalHost}/{key}";
        await overseerr.PostIssueComment(issue.Id, redirectUrl);
    }
}