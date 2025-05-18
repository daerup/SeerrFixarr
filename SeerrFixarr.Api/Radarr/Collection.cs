namespace SeerrFixarr.Api.Radarr;

public record Collection
{
    public string Title { get; init; } = null!;
    public int TmdbId { get; init; }
}