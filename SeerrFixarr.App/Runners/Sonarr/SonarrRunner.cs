using CSharpFunctionalExtensions;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Sonarr;
using Serilog;

namespace SeerrFixarr.App.Runners.Sonarr;

public class
    SonarrRunner(IOverseerrApi overseerr, ISonarrApi sonarr, ITimeOutProvider timeOutProvider)
{
    public async Task HandleEpisodeIssue(Issue issue)
    {
        await (GetTarget(issue) switch
        {
            IssueTargetSonarr.SpecificEpisode => HandleSpecificEpisode(issue),
            IssueTargetSonarr.WholeSeason => HandleWholeSeason(issue),
            IssueTargetSonarr.AllSeasons => HandleAllSeasons(issue),
        });
    }

    private async Task HandleSpecificEpisode(Issue issue)
    {
        var seasonEpisodeString = issue.GetIdentifier();
        var (episode, episodeFile) = await GetEpisodeFromIssue(issue);

        await episode.Match(
            async e =>
            {
                await DeleteEpisodeAsync(issue, episodeFile);
                await timeOutProvider.AwaitFileDeletion();
                await GrabEpisode(e, issue);
            },
            async () => await SkipFixing(issue, seasonEpisodeString));
    }

    private async Task SkipFixing(Issue issue, string episodeIdentifier)
    {
        await overseerr.PostIssueComment(issue.Id, ShowTranslationExtensions.EpisodeNotFoundMessage(episodeIdentifier));
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private async Task HandleAllSeasons(Issue issue)
    {
        await overseerr.PostIssueComment(issue.Id, ShowTranslationExtensions.WholeShowFaultyMessage());
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private async Task HandleWholeSeason(Issue issue)
    {
        await overseerr.PostIssueComment(issue.Id, ShowTranslationExtensions.WholeSeasonFaultyMessage());
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private static IssueTargetSonarr GetTarget(Issue issue)
    {
        return issue switch
        {
            { ProblemEpisode: > 0, ProblemEpisode: > 0 } => IssueTargetSonarr.SpecificEpisode,
            { ProblemSeason: > 0, ProblemEpisode: 0 } => IssueTargetSonarr.WholeSeason,
            { ProblemSeason: 0, ProblemEpisode: 0 } => IssueTargetSonarr.AllSeasons,
            _ => throw new ArgumentOutOfRangeException($"Unknown issue type: {issue}")
        };
    }

    private async Task<(Maybe<Episode>, Maybe<EpisodeFile>)> GetEpisodeFromIssue(Issue issue)
    {
        var episodeNumber = issue.ProblemEpisode!.Value;
        var seasonNumber = issue.ProblemSeason!.Value;
        var episodes = await sonarr.GetEpisodes(issue.Media.Id, seasonNumber);
        var episode = episodes.SingleOrDefault(e => e.EpisodeNumber == episodeNumber).AsMaybe();
        var episodeFile = episode.Bind(e => e.EpisodeFile.AsMaybe());
        return (episode, episodeFile);
    }

    private async Task GrabEpisode(Episode episode, Issue issue)
    {
        var alreadyGrabbed = (await sonarr.GetDownloadQueueOfEpisodes([episode.Id])).FirstOrDefault().AsMaybe();
        if (alreadyGrabbed.HasValue)
        {
            await HandleDownloadAlreadyInProgress(issue, alreadyGrabbed.Value);
            return;
        }

        await sonarr.AutomaticGrabEpisode(episode.Id);
        await timeOutProvider.AwaitDownloadQueueUpdated();

        await CheckIfGrabbed(episode, issue);
    }

    private async Task CheckIfGrabbed(Episode episode, Issue issue, int retryCount = 0)
    {
        var grabbed = (await sonarr.GetDownloadQueueOfEpisodes([episode.Id])).FirstOrDefault().AsMaybe();
        await grabbed.Match(
            async f => await Grabbed(issue, f),
            async () => await NotGrabbed(episode, issue, retryCount)
        );
    }

    private async Task Grabbed(Issue issue, EpisodeDownload file)
    {
        await overseerr.PostIssueComment(issue.Id, file.GrabbedMessage());
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private async Task NotGrabbed(Episode episode, Issue issue, int retryCount)
    {
        await timeOutProvider.AwaitDownloadQueueUpdated();
        if (retryCount >= 3)
        {
            Log.Information("Could not grab episode {identifier} after 3 attempts, closing issue...", issue.GetIdentifier());
            await overseerr.PostIssueComment(issue.Id, ShowTranslationExtensions.EpisodeNotGrabbedMessage(issue.GetIdentifier()));
            await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
            return;
        }
        
        Log.Information("Could not grab episode {identifier}, retrying...", issue.GetIdentifier());
        await CheckIfGrabbed(episode, issue, ++retryCount);
    }

    private async Task HandleDownloadAlreadyInProgress(Issue issue, EpisodeDownload alreadyGrabbed)
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
            async file => await DeleteFile(issue, file, identifier),
            async () => await overseerr.PostIssueComment(issue.Id,
                ShowTranslationExtensions.NoEpisodeFileToDeleteMessage(identifier))
        );
    }

    private async Task DeleteFile(Issue issue, EpisodeFile file, string episodeIdentifier)
    {
        await overseerr.PostIssueComment(issue.Id, file.DeletionStartedMessage(episodeIdentifier));
        await sonarr.DeleteEpisodeFile(file.Id);
        await overseerr.PostIssueComment(issue.Id, file.DeletionFinishedMessage(episodeIdentifier));
    }
}