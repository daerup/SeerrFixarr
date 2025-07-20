using System.Diagnostics;
using CSharpFunctionalExtensions;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Sonarr;
using SeerrFixarr.App.Shared;
using Serilog;

namespace SeerrFixarr.App.Runners.Sonarr;

public class SonarrRunner(
    IssueTargetInformationExtractor issueTargetInformationExtractor,
    IOverseerrApi overseerr,
    ISonarrApi sonarr,
    ITimeOutProvider timeOutProvider)
{
    public async Task HandleEpisodeIssueAsync(Issue issue)
    {
        var info = await issueTargetInformationExtractor.ExtractFromAsync(issue);
        await (info.TargetType switch
        {
            IssueTarget.Episode => HandleSpecificEpisodeAsync(issue),
            IssueTarget.Season => HandleWholeSeasonAsync(issue),
            IssueTarget.Show => HandleAllSeasonsAsync(issue),
            IssueTarget.Movie => throw new UnreachableException(),
        });
    }

    private async Task HandleSpecificEpisodeAsync(Issue issue)
    {
        var seasonEpisodeString = issue.GetIdentifier();
        var (episode, episodeFile) = await GetEpisodeFromIssueAsync(issue);

        await episode.Match(
            async e =>
            {
                await DeleteEpisodeAsync(issue, episodeFile);
                await timeOutProvider.AwaitFileDeletionAsync();
                await GrabEpisodeAsync(e.Id, issue);
            },
            async () => await SkipFixingAsync(issue, seasonEpisodeString));
    }

    private async Task SkipFixingAsync(Issue issue, string episodeIdentifier)
    {
        await overseerr.PostIssueComment(issue.Id, ShowTranslationExtensions.EpisodeNotFoundMessage(episodeIdentifier));
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private async Task HandleAllSeasonsAsync(Issue issue)
    {
        await overseerr.PostIssueComment(issue.Id, ShowTranslationExtensions.WholeShowFaultyMessage());
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private async Task HandleWholeSeasonAsync(Issue issue)
    {
        await overseerr.PostIssueComment(issue.Id, ShowTranslationExtensions.WholeSeasonFaultyMessage());
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private async Task<(Maybe<Episode>, Maybe<EpisodeFile>)> GetEpisodeFromIssueAsync(Issue issue)
    {
        var episode = await sonarr.GetEpisodeFromIssueAsync(issue);
        var episodeFile = episode.Bind(e => e.EpisodeFile.AsMaybe());
        return (episode, episodeFile);
    }

    private async Task GrabEpisodeAsync(int episodeId, Issue issue)
    {
        var alreadyGrabbed = (await sonarr.GetDownloadQueueOfEpisodes([episodeId])).FirstOrDefault().AsMaybe();
        if (alreadyGrabbed.HasValue)
        {
            await HandleDownloadAlreadyInProgressAsync(issue, alreadyGrabbed.Value);
            return;
        }

        await sonarr.AutomaticGrabEpisode(episodeId);
        await timeOutProvider.AwaitDownloadQueueUpdatedAsync();

        await CheckIfGrabbedAsync(episodeId, issue);
    }

    private async Task CheckIfGrabbedAsync(int episodeId, Issue issue, int retryCount = 0)
    {
        var grabbed = (await sonarr.GetDownloadQueueOfEpisodes([episodeId])).FirstOrDefault().AsMaybe();
        await grabbed.Match(
            async f => await GrabbedAsync(issue, f),
            async () => await NotGrabbedAsync(episodeId, issue, retryCount)
        );
    }

    private async Task GrabbedAsync(Issue issue, EpisodeDownload file)
    {
        await overseerr.PostIssueComment(issue.Id, file.GrabbedMessage());
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private async Task NotGrabbedAsync(int episodeId, Issue issue, int retryCount)
    {
        await timeOutProvider.AwaitDownloadQueueUpdatedAsync();
        if (retryCount >= 5)
        {
            Log.Information("Could not grab episode {identifier} after 3 attempts, closing issue...",
                            issue.GetIdentifier());
            await overseerr.PostIssueComment(
                issue.Id, ShowTranslationExtensions.EpisodeNotGrabbedMessage(issue.GetIdentifier()));
            await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
            return;
        }

        Log.Information("Could not grab episode {identifier}, retrying...", issue.GetIdentifier());
        await CheckIfGrabbedAsync(episodeId, issue, ++retryCount);
    }

    private async Task HandleDownloadAlreadyInProgressAsync(Issue issue, EpisodeDownload alreadyGrabbed)
    {
        var episodeIdentifier = issue.GetIdentifier();
        await overseerr.PostIssueComment(issue.Id, alreadyGrabbed.AlreadyGrabbedMessage(episodeIdentifier));
        await overseerr.PostIssueComment(issue.Id, issue.CloseMessage());
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private async Task DeleteEpisodeAsync(Issue issue, Maybe<EpisodeFile> episodeFile)
    {
        var identifier = issue.GetIdentifier();
        await episodeFile.Match(
            async file => await DeleteFileAsync(issue, file, identifier),
            async () => await overseerr.PostIssueComment(issue.Id,
                                                         ShowTranslationExtensions.NoEpisodeFileToDeleteMessage(
                                                             identifier))
        );
    }

    private async Task DeleteFileAsync(Issue issue, EpisodeFile file, string episodeIdentifier)
    {
        await overseerr.PostIssueComment(issue.Id, file.DeletionStartedMessage(episodeIdentifier));
        await sonarr.DeleteEpisodeFile(file.Id);
        await overseerr.PostIssueComment(issue.Id, file.DeletionFinishedMessage(episodeIdentifier));
    }
}