namespace SeerrFixarr.Api.Radarr;

public record SearchMovieRequest
{
    public string Name => "MoviesSearch";
    public int[] MovieIds { get; init; } = null!;
    public static implicit operator SearchMovieRequest(int movieId) => new() { MovieIds = [movieId] };
}