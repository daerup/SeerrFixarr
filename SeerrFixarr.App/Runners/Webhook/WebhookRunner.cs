using System.Globalization;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;

namespace SeerrFixarr.App.Runners.Webhook;

internal class WebhookRunner(IOverseerrApi overseerr, RadarrRunner radarrRunner, SonarrRunner sonarrRunner)
{
    public async Task RunAsync(WebhookIssueRoot body)
    {
        var issue = await overseerr.GetIssue(body.IssueId);
        var (username, locale) = await overseerr.GetLocalSettingsOfUser(issue.CreatedBy.Id);
        LocalToCultureSetter.SetCulture(username, locale);
        var task = issue.Media.MediaType switch
        {
            MediaType.Movie => radarrRunner.HandleMovieIssue(issue),
            MediaType.Tv => sonarrRunner.HandleEpisodeIssue(issue),
            _ => Task.CompletedTask
        };
        
        await task;
    }
}

internal class LocalToCultureSetter
{
    public static void SetCulture(string username, string locale)
    { 
        var culture = new CultureInfo(locale);
        Translations.Culture = culture;

    }
}