using System.Threading.Tasks;
using Refit;
using SeerrFixarr.Api.Shared;

namespace SeerrFixarr.Api.Radarr;

public interface IRadarrApi
{
    [Get("/movie/{id}")]
    Task<Movie> GetMovie(int id);

    [Get("/movies")]
    Task<Movie[]> GetMovies(int tmdbId);

    [Delete("/moviefile/{id}")]
    Task DeleteMovieFile([AliasAs("id")] int movieFileId);

    [Get("/release")]
    Task<InteractiveRelease[]> GetMovieReleases(int movieId);

    [Post("/release")]
    Task InteractiveGrabMovie(string guid, int indexerId);

    [Post("/command")]
    Task AutomaticGrabMovie([Body] SearchMovieRequest movieId);

    [Get("/queue/details")]
    Task<MovieDownload[]> GetDownloadQueue(int movieId);
}