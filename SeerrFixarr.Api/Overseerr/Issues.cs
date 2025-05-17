namespace SeerrFixarr.Api.Overseerr;

public record Issues 
{
    public PageInfo PageInfo { get; init; }
    public IssueId[] Results { get; init; }
}