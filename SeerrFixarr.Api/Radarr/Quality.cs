namespace SeerrFixarr.Api.Radarr;

public record Quality
{
    public string Name { get; init; } = null!;
    public int Resolution { get; init; }
}