namespace SeerrFixarr.Api.Overseerr;

public record Issues
{
    public PageInfo PageInfo { get; init; } = null!;
    public IssueId[] Results { get; init; } = [];
}