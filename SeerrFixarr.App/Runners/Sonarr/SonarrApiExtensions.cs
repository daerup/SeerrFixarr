using CSharpFunctionalExtensions;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Sonarr;

namespace SeerrFixarr.App.Runners.Sonarr;

public static class SonarrApiExtensions
{
    public static async Task<Maybe<Episode>> GetEpisodeFromIssueAsync(this ISonarrApi sonarr, Issue issue)
    {
        if (issue is not { ProblemEpisode: { } episode, ProblemSeason: { } season })
        {
            return Maybe<Episode>.None;
        }
        
        var seriesId = issue.Media.Id;
        var episodes = await sonarr.GetEpisodes(seriesId, season);
        return episodes.SingleOrDefault(e => e.EpisodeNumber == episode).AsMaybe();
    }
}