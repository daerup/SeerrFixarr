namespace SeerrFixarr.Api.Radarr;

public record MovieFile
{
    public int Id { get; init; }
    public string Path { get; init; } = null!;
    public long Size { get; init; }
}