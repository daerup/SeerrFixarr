using CSharpFunctionalExtensions;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Sonarr;

namespace SeerrFixarr.App;

public class SonarrRunner(IOverseerrApi overseerr, ISonarrApi soanarr, ITimeOutProvider timeOutProvider, FileSizeFormatter fileSizeFormatter)
{
    public async Task HandleEpisodeIssue(Issue issue)
    {
        var task = GetTarget(issue) switch
        {
            IssueTargetSonarr.SpecificEpisode => HandleSpecificEpisode(issue),
            IssueTargetSonarr.WholeSeason => HandleWholeSeason(issue),
            IssueTargetSonarr.AllSeasons => HandleAllSeasons(issue),
            _ => Task.CompletedTask
        };
        await task;
    }

    private async Task HandleSpecificEpisode(Issue issue)
    {
        var seasonEpisodeString = SeasonEpisodeString(issue);
        var (episode, episodefile) = await GetEpisodeFileFromIssue(issue);
        
        await episode.Match( 
            async e =>
            {
                await DeleteEpisodeAsync(issue, episodefile);
                await timeOutProvider.AwaitFileDeletion();
                await GrabEpisode(e, issue);
            },
            async () => await SkipFixing(issue, seasonEpisodeString));
    }

    private async Task SkipFixing(Issue issue, string seasonEpisodeString)
    {
        await overseerr.PostIssueComment(issue.Id, @$"❌ {seasonEpisodeString} not found, cannot automatically fix this issue");
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private async Task HandleAllSeasons(Issue issue)
    {
        await overseerr.PostIssueComment(issue.Id,  @"❌ Whole shows cannot be automatically fixed. Please recreate the issue with a specific episode.");
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }
    
    private async Task HandleWholeSeason(Issue issue)
    {
        await overseerr.PostIssueComment(issue.Id,  @"❌ Whole seasons cannot be automatically fixed. Please recreate the issue with a specific episode.");
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private static IssueTargetSonarr GetTarget(Issue issue)
    {
        return issue switch 
        {
            { ProblemEpisode: > 0, ProblemEpisode: > 0 } => IssueTargetSonarr.SpecificEpisode,
            { ProblemSeason: > 0, ProblemEpisode: 0  } => IssueTargetSonarr.WholeSeason,
            { ProblemSeason: 0, ProblemEpisode: 0 } => IssueTargetSonarr.AllSeasons,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task<(Maybe<Episode>, Maybe<EpisodeFile>)> GetEpisodeFileFromIssue(Issue issue)
    {
        var episodeNumber = issue.ProblemEpisode!.Value;
        var seasonNumber = issue.ProblemSeason!.Value;
        var episodes = await soanarr.GetEpisodes(issue.Media.Id, seasonNumber);
        var episode = episodes.SingleOrDefault(e => e.EpisodeNumber == episodeNumber).AsMaybe();
        return (episode, episode.Select(e => e.EpisodeFile));
    }
    
    private async Task GrabEpisode(Episode episode, Issue issue)
    {
        var seasonEpisodeString = SeasonEpisodeString(issue);
        
        var alreadyGrabbed = (await soanarr.GetDownloadQueueOfEpisodes([episode.Id])).FirstOrDefault().AsMaybe();
        if (alreadyGrabbed.HasValue)
        {
            await overseerr.PostIssueComment(issue.Id, @$"⬇️ Already grabbed file for '{seasonEpisodeString}'. 🕒 {alreadyGrabbed.Value.EstimatedCompletionTime.ToLocalTime()}'");
            await overseerr.PostIssueComment(issue.Id, @"✅️ This issue will be closed. Reopen if the problem persists.");
            await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
            return;
        }

        await soanarr.GrabEpisode(episode.Id);
        await timeOutProvider.AwaitDownloadQueueUpdated();
        
        var grabbed = (await soanarr.GetDownloadQueueOfEpisodes([episode.Id])).FirstOrDefault().AsMaybe();
        await grabbed.Match(async f =>
            {
                var fileSize = fileSizeFormatter.GetFileSize(f.Size);
                var comment = @$"⬇️ Grabbed file '{f.Title}' 💾 {fileSize} 🕒 {f.EstimatedCompletionTime.ToLocalTime()}";
                await overseerr.PostIssueComment(issue.Id, comment);
                await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
            },
            async () =>
            {
                await overseerr.PostIssueComment(issue.Id, @$"🥺 Could not grab file of '{seasonEpisodeString}'");
            });
    }

    private async Task DeleteEpisodeAsync(Issue issue, Maybe<EpisodeFile> episodefile)
    {
        var seasonEpisodeString = SeasonEpisodeString(issue);
        await episodefile.Match(async episodeFile =>
            {
                var fileSize = fileSizeFormatter.GetFileSize(episodeFile.Size);
                await overseerr.PostIssueComment(issue.Id, @$"🗑️ Deleting file of '{seasonEpisodeString}' ({fileSize})");
                await soanarr.DeleteEpisodeFile(episodeFile.Id);
                await overseerr.PostIssueComment(issue.Id, @$"✅ Successfully deleted episode file ({fileSize})");
            },
            async () =>
            {
                await overseerr.PostIssueComment(issue.Id, @$"⏩ Episode file not found for '{seasonEpisodeString}', skipping deletion");
            });
    }

    private string SeasonEpisodeString(Issue issue)
    {
        var episodeNumber = issue.ProblemEpisode!.Value;
        var seasonNumber = issue.ProblemSeason!.Value;
        return $"S{seasonNumber:D2}E{episodeNumber:D2}";
    }
}