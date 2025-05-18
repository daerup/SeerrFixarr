namespace SeerrFixarr.Api.Sonarr;

public class EpisodeFile
{
    public string Path { get; init; } = null!;
    public long Size { get; init; }
    public int Id { get; init; }
}