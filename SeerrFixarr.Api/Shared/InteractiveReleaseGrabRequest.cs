namespace SeerrFixarr.Api.Shared;

public record InteractiveReleaseGrabRequest
{
    public string Guid { get; init; } = null!;
    public int IndexerId { get; init; }
    
    public static InteractiveReleaseGrabRequest FromRelease(InteractiveRelease release) => new()
    {
        Guid = release.Guid,
        IndexerId = release.IndexerId
    };
}