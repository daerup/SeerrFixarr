using System;

namespace SeerrFixarr.Api.Sonarr;

public class EpisodeDownload
{
    public int Id { get; init; }
    public int SeriesId { get; init; }
    public int EpisodeId { get; init; }
    public long Size { get; init; }
    public string Title { get; init; } = null!;
    public DateTime EstimatedCompletionTime { get; init; }
    public string Status { get; init; } = null!;
}