using CSharpFunctionalExtensions;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Sonarr;
using SeerrFixarr.App.Runners.Sonarr;
using SeerrFixarr.App.Shared;

namespace SeerrFixarr.App.Runners;

public record IssueTargetInformation(IssueTarget TargetType, int Id);

public class IssueTargetInformationExtractor(ISonarrApi sonarr)
{
    public async Task<IssueTargetInformation> ExtractFromAsync(Issue issue)
    {
        var target = GetTarget(issue);
        var id = await GetIdAsync(target, issue);
        return new IssueTargetInformation(target, id);
    }

    private IssueTarget GetTarget(Issue issue)
    {
        return issue switch
        {
            { Media.MediaType: MediaType.Movie } => IssueTarget.Movie,
            { ProblemSeason: >= 0, ProblemEpisode: > 0 } => IssueTarget.Episode,
            { ProblemSeason: > 0, ProblemEpisode: 0 } => IssueTarget.Season,
            { ProblemSeason: 0, ProblemEpisode: 0 } => IssueTarget.Show,
            _ => throw new ArgumentOutOfRangeException($"Unknown issue type: {issue}")
        };
    }

    private async Task<int> GetIdAsync(IssueTarget target, Issue issue)
    {
        return target switch
        {
            IssueTarget.Movie => issue.Media.Id,
            IssueTarget.Episode => (await sonarr.GetEpisodeFromIssueAsync(issue)).Map(i => i.Id).GetValueOrThrow(),
            IssueTarget.Season => issue.Media.Id,
            IssueTarget.Show => issue.Media.Id,
        };
    }
}