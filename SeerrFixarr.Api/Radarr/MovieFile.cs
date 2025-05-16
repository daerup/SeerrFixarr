namespace SeerrFixarr.Api.Radarr;

public record MovieFile
{
    public int Id { get; init; }
    public string RelativePath { get; init; }
    public string Path { get; init; }
    public long Size { get; init; }
}