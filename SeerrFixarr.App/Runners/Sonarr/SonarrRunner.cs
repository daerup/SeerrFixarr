using CSharpFunctionalExtensions;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Sonarr;

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

  private async Task SkipFixing(Issue issue, string seasonEpisodeString)
  {
    await overseerr.PostIssueComment(issue.Id,
      @$"❌ {seasonEpisodeString} not found, cannot automatically fix this issue");
    await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
  }

  private async Task HandleAllSeasons(Issue issue)
  {
    await overseerr.PostIssueComment(issue.Id,
      @"❌ Whole shows cannot be automatically fixed. Please recreate the issue with a specific episode.");
    await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
  }

  private async Task HandleWholeSeason(Issue issue)
  {
    await overseerr.PostIssueComment(issue.Id,
      @"❌ Whole seasons cannot be automatically fixed. Please recreate the issue with a specific episode.");
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

    await sonarr.GrabEpisode(episode.Id);
    await timeOutProvider.AwaitDownloadQueueUpdated();

    var grabbed = (await sonarr.GetDownloadQueueOfEpisodes([episode.Id])).FirstOrDefault().AsMaybe();
    await grabbed.Match(
      async f => await Grabbed(issue, f),
      async () => await overseerr.PostIssueComment(issue.Id, @$"🥺 Could not grab file of '{issue.GetIdentifier()}'")
    );
  }

  private async Task HandleDownloadAlreadyInProgress(Issue issue, EpisodeDownload alreadyGrabbed)
  {
    var episodeIdentifier = issue.GetIdentifier();
    await overseerr.PostIssueComment(issue.Id, @$"⬇️ Already grabbed file for '{episodeIdentifier}'. 🕒 {alreadyGrabbed.EstimatedCompletionTime.ToLocalTime()}'");
    await overseerr.PostIssueComment(issue.Id, @"✅️ This issue will be closed. Reopen if the problem persists.");
    await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
  }

  private async Task Grabbed(Issue issue, EpisodeDownload file)
  {
    var comment = @$"⬇️ Grabbed file '{file.Title}' 💾 {file.GetReadableFileSize()} 🕒 {file.EstimatedCompletionTime.ToLocalTime()}";
    await overseerr.PostIssueComment(issue.Id, comment);
    await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
  }

  private async Task DeleteEpisodeAsync(Issue issue, Maybe<EpisodeFile> episodeFile)
  {
    var identifier = issue.GetIdentifier();
    await episodeFile.Match(
      async file => await DeleteFile(issue, file, identifier),
      async () => await overseerr.PostIssueComment(issue.Id, @$"⏩ Episode file not found for '{identifier}', skipping deletion")
    );
  }

  private async Task DeleteFile(Issue issue, EpisodeFile file, string episodeIdentifier)
  {
    await overseerr.PostIssueComment(issue.Id, @$"🗑️ Deleting file of '{episodeIdentifier}' ({file.GetReadableFileSize()})");
    await sonarr.DeleteEpisodeFile(file.Id);
    await overseerr.PostIssueComment(issue.Id, @$"✅ Successfully deleted episode file ({file.GetReadableFileSize()})");
  }
}