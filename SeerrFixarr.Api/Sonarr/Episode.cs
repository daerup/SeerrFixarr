namespace SeerrFixarr.Api.Sonarr;

public record Episode
{
    public int Id { get; init; }
    public int SeriesId { get; init; }
    public int SeasonNumber { get; init; }
    public int EpisodeNumber { get; init; }
    public string Title { get; init; } = null!;
    public int TvdbId { get; init; }
    public DateTime AirDateUtc { get; init; }
    public bool HasFile { get; init; }
    public bool Monitored { get; init; }
    public EpisodeFile? EpisodeFile { get; init; }
}