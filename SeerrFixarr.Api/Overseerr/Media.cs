namespace SeerrFixarr.Api.Overseerr;

public record Media
{
    public int Id { get; init; }
    public MediaType MediaType { get; init; }
    public int? TmdbId { get; init; }
    public int? TvdbId { get; init; }
    public int? ImdbId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? PlexUrl { get; init; }
    public string? IOsPlexUrl { get; init; }
}