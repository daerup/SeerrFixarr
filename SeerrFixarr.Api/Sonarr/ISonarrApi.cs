using Refit;
using SeerrFixarr.Api.Shared;

namespace SeerrFixarr.Api.Sonarr;

public interface ISonarrApi
{
    [Get("/episode/{id}")]
    Task<Episode> GetEpisode([AliasAs("id")] int episodeId);
    
    [Get("/episode")] 
    Task<Episode[]> GetEpisodes(int seriesId, int seasonNumber, bool includeEpisodeFile = true);
    
    [Delete("/episodefile/{id}")] 
    Task DeleteEpisodeFile([AliasAs("id")] int episodeFileId);


    [Get("/release")]
    Task<InteractiveRelease[]> GetEpisodeReleases(int episodeId);

    [Post("/release")]
    Task InteractiveGrabEpisode(string guid, int indexerId);
    
    [Post("/command")]
    Task<string> AutomaticGrabEpisode([Body] SearchEpisodeRequest episodeId);
    
    [Post("/command")]
    Task AutomaticGrabSeries([Body] SearchSeriesRequest seriesId);
    
    [Get("/queue/details")]
    Task<EpisodeDownload[]> GetDownloadQueueOfSeries(int seriesId);
    
    [Get("/queue/details")]
    Task<EpisodeDownload[]> GetDownloadQueueOfEpisodes(int[] episodeIds);
}