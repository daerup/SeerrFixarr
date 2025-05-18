namespace SeerrFixarr.Api.Overseerr;

public record Issue
{
    public int Id { get; init; }
    public int Status { get; init; }
    public int? ProblemSeason { get; init; }
    public int? ProblemEpisode { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<Comment> Comments { get; init; } = [];
    public User CreatedBy { get; init; } = null!;
    public Media Media { get; init; } = null!;
    
    public string GetIdentifier()
    {
        return Media.MediaType switch
        {
            MediaType.Tv => CreateEpisodeIdentifier(),
            MediaType.Movie => CreateMovieIdentifier(),
        };
        
        string CreateEpisodeIdentifier() => $"S{ProblemSeason!.Value:D2}E{ProblemEpisode!.Value:D2}";
        string CreateMovieIdentifier() => $"{Media.TmdbId}";
    }

    public virtual bool Equals(Issue? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}