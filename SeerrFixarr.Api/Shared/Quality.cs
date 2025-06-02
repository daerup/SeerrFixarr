namespace SeerrFixarr.Api.Shared;

public record Quality
{
    public string Name { get; init; } = null!;
    public int Resolution { get; init; }
}