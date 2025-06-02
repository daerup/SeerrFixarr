using CSharpFunctionalExtensions;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Shared;
using UnitsNet;

namespace SeerrFixarr.Test;

internal class FakeRadarrApi : IRadarrApi
{
    public readonly List<Movie> Movies = [];
    public readonly List<MovieDownload> DownloadQueue = [];

    public Task<Movie> GetMovie(int id)
    {
        var movie = Movies.SingleWithApiException(m => m.Id == id);
        return Task.FromResult(movie);
    }

    public Task<Movie[]> GetMovies(int tmdbId)
    {
        var movies = Movies.Where(m => m.TmdbId == tmdbId).ToArray();
        return Task.FromResult(movies);
    }

    public Task DeleteMovieFile(int movieFileId)
    {
        var movie = Movies.SingleOrDefaultWithApiException(m => m.MovieFile?.Id == movieFileId).AsMaybe();
        movie.Execute(m =>
        {
            var updatedMovie = m with { MovieFile = null, HasFile = false };
            Movies.Add(updatedMovie);
            Movies.Remove(m);
        });
        return Task.CompletedTask;
    }

    public Task<InteractiveRelease[]> GetMovieReleases(int movieId)
    {
        throw new NotImplementedException();
    }

    public Task InteractiveGrabMovie(InteractiveReleaseGrabRequest request)
    {
        throw new NotImplementedException();
    }

    public Task AutomaticGrabMovie(SearchMovieRequest request)
    {
        var requestMovieId = request.MovieIds[0];
        var movie = Movies.SingleWithApiException(m => m.Id == requestMovieId);
        var download = ConvertToDownload(movie, requestMovieId);
        DownloadQueue.Add(download);
        return Task.CompletedTask;
    }

    private MovieDownload ConvertToDownload(Movie movie, int requestMovieId)
    {
        var fileDownloadName = movie.Title.Replace(" ", ".") + ".test.mkv";
        return new MovieDownload
        {
            Id = DownloadQueue.Count + 1,
            MovieId = requestMovieId,
            Title = fileDownloadName.ToLower(),
            EstimatedCompletionTime = TestDataBuilder.FakeTimeProvider.GetUtcNow().Add(TimeSpan.FromHours(1)).DateTime,
            Size = (long)Information.FromGibibytes(2).Bytes
        };
    }

    public Task<MovieDownload[]> GetDownloadQueue(int movieId)
    {
        var queue = DownloadQueue.Where(d => d.MovieId == movieId).ToArray();
        return Task.FromResult(queue);
    }

    public void Setup(Movie movie)
    {
        if (Movies.Any(m => m.Id == movie.Id))
        {
            throw new InvalidOperationException("Movie already exists");
        }

        Movies.Add(movie);
    }

    public void SetupDownloading(Movie movie)
    {
        Setup(movie);
        var download = ConvertToDownload(movie, movie.Id);
        DownloadQueue.Add(download);
    }
}