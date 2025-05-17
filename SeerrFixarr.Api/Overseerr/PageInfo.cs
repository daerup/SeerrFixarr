namespace SeerrFixarr.Api.Overseerr;

public record PageInfo
{
    public int Pages { get; init; }
    public int Results { get; init; }
    public int Page { get; init; }
}