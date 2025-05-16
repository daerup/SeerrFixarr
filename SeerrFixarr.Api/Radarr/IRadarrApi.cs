using Refit;

namespace SeerrFixarr.Api.Radarr;

public interface IRadarrApi
{
    [Get("/movie/{id}")]
    Task<Movie> GetMovie(int id);

    [Get("/movies")]
    Task<Movie[]> GetMovies(int tmdbId);

    [Delete("/moviefile/{id}")]
    Task DeleteMovieFile([AliasAs("id")] int movieFileId);

    [Post("/command")]
    Task GrabMovie([Body] SearchMovieRequest movieId);

    [Get("/queue/details")]
    Task<MovieDownload[]> GetDownloadQueue(int movieId);
}