namespace SeerrFixarr.Api.Overseerr;

public record Issue
{
    public int Id { get; init; }
    public int Status { get; init; }
    public int? ProblemSeason { get; init; }
    public int? ProblemEpisode { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<Comment> Comments { get; init; }
    public User CreatedBy { get; init; }
    public Media Media { get; init; }
}