namespace SeerrFixarr.Api.Radarr;

public record Revision
{
    public int Version { get; set; }
    public int Real { get; set; }
    public bool IsRepack { get; set; }
}