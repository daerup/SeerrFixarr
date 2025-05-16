namespace SeerrFixarr.Api.Radarr;

public record Movie
{
    public string Title { get; init; }
    public DateTime ReleaseDate { get; init; }
    public bool HasFile { get; init; }
    public bool Monitored { get; init; }
    public int TmdbId { get; init; }
    public DateTime Added { get; init; }
    public MovieFile MovieFile { get; init; }
    public Collection Collection { get; init; }
    public int Id { get; init; }
}