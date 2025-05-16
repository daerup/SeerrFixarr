namespace SeerrFixarr.Api.Radarr;

public record MovieDownload
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public long Size { get; set; }
    public string Title { get; set; }
    public DateTime EstimatedCompletionTime { get; set; }
    public string Status { get; set; }
}