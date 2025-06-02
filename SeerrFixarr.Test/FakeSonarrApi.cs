using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SeerrFixarr.Api.Sonarr;
using UnitsNet;

namespace SeerrFixarr.Test;

internal class FakeSonarrApi : ISonarrApi
{
  public readonly List<Episode> Episodes = [];
  public readonly List<EpisodeDownload> DownloadQueue = [];

  public Task<Episode> GetEpisode(int episodeId)
  {
    var episode = Episodes.SingleWithApiException(m => m.Id == episodeId);
    return Task.FromResult(episode);
  }

  public Task<Episode[]> GetEpisodes(int seriesId, int seasonNumber, bool includeEpisodeFile = true)
  {
    var episodes = Episodes.Where(m => m.SeriesId == seriesId && m.SeasonNumber == seasonNumber).ToArray();
    return Task.FromResult(episodes);
  }

  public Task DeleteEpisodeFile(int episodeFileId)
  {
    var episode = Episodes.SingleOrDefaultWithApiException(m => m.EpisodeFile?.Id == episodeFileId).AsMaybe();
    episode.Execute(e =>
    {
      var updatedEpisode = e with { EpisodeFile = null, HasFile = false };
      Episodes.Add(updatedEpisode);
      Episodes.Remove(e);
    });
    return Task.CompletedTask;
  }

  public Task<string> GrabEpisode(SearchEpisodeRequest episodeId)
  {
    var requestEpisodeId = episodeId.EpisodeIds[0];
    var episode = Episodes.SingleWithApiException(m => m.Id == requestEpisodeId);
    var download = ConvertToDownload(episode, requestEpisodeId);
    DownloadQueue.Add(download);
    return Task.FromResult("test");
  }

  public Task GrabSeries(SearchSeriesRequest seriesId)
  {
    var requestSeriesId = seriesId.SeriesId;
    const int numberOfEpisodesInFakeSeries = 10;

    var episodes = Enumerable.Range(1, numberOfEpisodesInFakeSeries).Select(e =>
      new Episode
      {
        Id = requestSeriesId + e,
        SeriesId = requestSeriesId,
        SeasonNumber = 1,
        EpisodeNumber = e,
        Title = $"Test Episode {e:N2}",
        AirDateUtc = DateTime.UtcNow.AddDays(e * 7),
        HasFile = false,
        Monitored = true
      }
    );
    Episodes.AddRange(episodes);
    return Task.CompletedTask;
  }

  private EpisodeDownload ConvertToDownload(Episode episode, int requestEpisodeId)
  {
    var fileDownloadName = episode.Title.Replace(" ", ".") + ".test.mkv";
    return new EpisodeDownload
    {
      Id = requestEpisodeId,
      EpisodeId = episode.Id,
      SeriesId = episode.SeriesId,
      Title = fileDownloadName.ToLower(),
      EstimatedCompletionTime = TestDataBuilder.FakeTimeProvider.GetUtcNow().Add(TimeSpan.FromHours(1)).DateTime,
      Size = (long)Information.FromGibibytes(2).Bytes
    };
  }

  public Task<EpisodeDownload[]> GetDownloadQueueOfSeries(int seriesId)
  {
    var queue = DownloadQueue.Where(d => d.SeriesId == seriesId).ToArray();
    return Task.FromResult(queue);
  }

  public Task<EpisodeDownload[]> GetDownloadQueueOfEpisodes(int[] episodeIds)
  {
    var queue = DownloadQueue.Where(d => episodeIds.Contains(d.EpisodeId)).ToArray();
    return Task.FromResult(queue);
  }

  public void CreateShow(params Episode[] episodes)
  {
    Episodes.AddRange(episodes);
  }

  public void Setup(Episode episode)
  {
    if (Episodes.Any(m => m.Id == episode.Id))
    {
      throw new InvalidOperationException("Episode already exists");
    }

    Episodes.Add(episode);
  }

  public void SetupDownloading(Episode episode)
  {
    Setup(episode);
    var download = ConvertToDownload(episode, episode.Id);
    DownloadQueue.Add(download);
  }
}