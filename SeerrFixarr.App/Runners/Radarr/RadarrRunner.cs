using CSharpFunctionalExtensions;
using Refit;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using Serilog;

namespace SeerrFixarr.App.Runners.Radarr;

public class RadarrRunner(
  IOverseerrApi overseerr,
  IRadarrApi radarr,
  ITimeOutProvider timeOutProvider)
{
  public async Task HandleMovieIssueAsync(Issue issue)
  {
    var (movie, movieFile) = await GetMovieFromIssueAsync(issue);
    await DeleteMovieAsync(issue, movie, movieFile);
    await timeOutProvider.AwaitFileDeletionAsync();
    await GrabMovieAsync(movie, issue);
  }

  private async Task<(Movie, Maybe<MovieFile>)> GetMovieFromIssueAsync(Issue issue)
  {
    try
    {
      var movie = await radarr.GetMovie(issue.Media.Id);
      return (movie, movie.MovieFile);
    }
    catch (ApiException)
    {
      try
      {
        var movie = (await radarr.GetMovies(issue.Media.TmdbId!.Value)).Single();
        return (movie, movie.MovieFile);
      }
      catch (Exception)
      {
        await overseerr.PostIssueComment(issue.Id, issue.MovieNotFoundMessage());
        throw;
      }
    }
  }

  private async Task GrabMovieAsync(Movie movie, Issue issue)
  {
    var alreadyGrabbed = (await radarr.GetDownloadQueue(movie.Id)).FirstOrDefault().AsMaybe();
    if (alreadyGrabbed.HasValue)
    {
      await overseerr.PostIssueComment(issue.Id, alreadyGrabbed.Value.AlreadyGrabbedMessage());
      await overseerr.PostIssueComment(issue.Id, issue.CloseMessage());
      await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
      return;
    }

    await radarr.AutomaticGrabMovie(movie.Id);

    await timeOutProvider.AwaitDownloadQueueUpdatedAsync();

    await CheckIfGrabbedAsync(movie, issue);
  }

  private async Task CheckIfGrabbedAsync(Movie movie, Issue issue, int retryCount = 0)
  {
      var queue = await radarr.GetDownloadQueue(movie.Id);
      var grabbed = queue.FirstOrDefault().AsMaybe();
      await grabbed.Match(
          async file => await GrabbedAsync(issue, file),
          async () => await NotGrabbedAsync(movie, issue, retryCount)
      );
  }

  private async Task GrabbedAsync(Issue issue, MovieDownload file)
  {
    await overseerr.PostIssueComment(issue.Id, file.GrabbedMessage());
    await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
  }
  
  private async Task NotGrabbedAsync(Movie movie, Issue issue, int retryCount)
  {
      await timeOutProvider.AwaitDownloadQueueUpdatedAsync();
      if (retryCount >= 5)
      {
          Log.Information("Could not grab episode {identifier} after 3 attempts, closing issue...", issue.GetIdentifier());
          await overseerr.PostIssueComment(issue.Id, movie.NotGrabbedMessage());
          await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
          return;
      }
        
      Log.Information("Could not grab episode {identifier}, retrying...", issue.GetIdentifier());
      await CheckIfGrabbedAsync(movie, issue, ++retryCount);
  }

  private async Task DeleteMovieAsync(Issue issue, Movie movie, Maybe<MovieFile> file)
  {
    await file.Match(
      async f => await DeleteFileAsync(issue, movie, f),
      async () => await overseerr.PostIssueComment(issue.Id, movie.NoFileToDeleteMessage()));
  }

  private async Task DeleteFileAsync(Issue issue, Movie movie, MovieFile file)
  {
    await overseerr.PostIssueComment(issue.Id, movie.DeletionStartedMessage(file));
    await radarr.DeleteMovieFile(file.Id);
    await overseerr.PostIssueComment(issue.Id, file.DeletionFinishedMessage());
  }
}