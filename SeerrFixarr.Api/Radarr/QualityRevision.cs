namespace SeerrFixarr.Api.Radarr;

public record QualityRevision
{
    public Quality Quality { get; init; } = null!;
    public Revision Revision { get; init; } = null!;
}