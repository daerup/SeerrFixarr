using System.Threading.Tasks;
using Refit;

namespace SeerrFixarr.Api.Sonarr;

public interface ISonarrApi
{
    [Get("/episode/{id}")]
    Task<Episode> GetEpisode([AliasAs("id")] int episodeId);
    
    [Get("/episode")] 
    Task<Episode[]> GetEpisodes(int seriesId, int seasonNumber, bool includeEpisodeFile = true);
    
    [Delete("/episodefile/{id}")] 
    Task DeleteEpisodeFile([AliasAs("id")] int episodeFileId);

    [Post("/command")]
    Task<string> GrabEpisode([Body] SearchEpisodeRequest episodeId);
    
    [Post("/command")]
    Task GrabSeries([Body] SearchSeriesRequest seriesId);
    
    [Get("/queue/details")]
    Task<EpisodeDownload[]> GetDownloadQueueOfSeries(int seriesId);
    
    [Get("/queue/details")]
    Task<EpisodeDownload[]> GetDownloadQueueOfEpisodes(int[] episodeIds);
}