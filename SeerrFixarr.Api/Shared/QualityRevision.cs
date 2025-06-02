namespace SeerrFixarr.Api.Shared;

public record QualityRevision
{
    public Quality Quality { get; init; } = null!;
    public Revision Revision { get; init; } = null!;
}