using System.Collections.Generic;

namespace SeerrFixarr.Api.Radarr;

public record MovieRelease
{
    public string Guid { get; init; } = null!;
    public string InfoUrl { get; init; } = null!;
    public QualityRevision Quality { get; init; } = null!;
    public int CustomFormatScore { get; init; }
    public int Age { get; init; }
    public double AgeHours { get; init; }
    public double AgeMinutes { get; init; }
    public long Size { get; init; }
    public int IndexerId { get; init; }
    public string Indexer { get; init; } = null!;
    public string Title { get; init; } = null!;
    public List<string> MovieTitles { get; init; } = [];
    public List<Language> Languages { get; init; } = [];
    public int MappedMovieId { get; init; }
    public int TmdbId { get; init; }
    public int ImdbId { get; init; }
    public List<string> Rejections { get; init; } = [];
    public DownloadProtocol DownloadProtocol { get; init; }
}