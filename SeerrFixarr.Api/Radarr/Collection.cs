namespace SeerrFixarr.Api.Radarr;

public record Collection
{
    public string Title { get; init; }
    public int TmdbId { get; init; }
}