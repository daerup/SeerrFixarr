using Refit;

namespace SeerrFixarr.Api.Sonarr;

public interface ISonarrApi
{
    [Get("/episode/{id}")]
    Task GetEpisode([AliasAs("id")] int episodeId);
}