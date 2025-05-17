using CSharpFunctionalExtensions;
using SeerrFixarr.Api.Radarr;

namespace SeerrFixarr.Test;

internal class FakeRadarrApi : IRadarrApi
{
    private readonly List<Movie> _movies = [];
    private readonly List<MovieDownload> downlaodQueue = [];

    public Task<Movie> GetMovie(int id)
    {
        var movie = _movies.Single(m => m.Id == id);
        return Task.FromResult(movie);
    }

    public Task<Movie[]> GetMovies(int tmdbId)
    {
        var movies = _movies.Where(m => m.TmdbId == tmdbId).ToArray();
        return Task.FromResult(movies);
    }

    public Task DeleteMovieFile(int movieFileId)
    {
        var movie = _movies.SingleOrDefault(m => m.MovieFile?.Id == movieFileId).AsMaybe();
        movie.Execute(m =>
        {
            var updatedMovie = m with { MovieFile = null, HasFile = false };
            _movies.Add(updatedMovie);
            _movies.Remove(m);
        });
        return Task.CompletedTask;
    }

    public Task GrabMovie(SearchMovieRequest request)
    {
        var movie = _movies.Single(m => m.Id == request.MovieIds[0]);
        var fileDownloadName = movie.Title.Replace(" ", ".") + ".test.mkv";
        var download = new MovieDownload
        {
            Id = downlaodQueue.Count + 1,
            Title = fileDownloadName,
            EstimatedCompletionTime = DateTime.UtcNow.AddHours(1),
            Size = 100L,
        };
        downlaodQueue.Add(download);
        return Task.CompletedTask;
    }

    public Task<MovieDownload[]> GetDownloadQueue(int movieId)
    {
        var queue = downlaodQueue.Where(d => d.Id == movieId).ToArray();
        return Task.FromResult(queue);
    }

    public void Setup(Movie movie)
    {
        if (_movies.Any(m => m.Id == movie.Id))
        {
            throw new InvalidOperationException("Movie already exists");
        }
        _movies.Add(movie);
    }
}