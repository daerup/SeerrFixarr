using CSharpFunctionalExtensions;
using Refit;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;

namespace SeerrFixarr.App.Runners.Radarr;

public class RadarrRunner(
  IOverseerrApi overseerr,
  IRadarrApi radarr,
  ITimeOutProvider timeOutProvider)
{
  public async Task HandleMovieIssue(Issue issue)
  {
    var (movie, moviefile) = await GetMovieFromIssue(issue);
    await DeleteMovieAsync(issue, movie, moviefile);
    await timeOutProvider.AwaitFileDeletion();
    await GrabMovie(movie, issue);
  }

  private async Task<(Movie, Maybe<MovieFile>)> GetMovieFromIssue(Issue issue)
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

  private async Task GrabMovie(Movie movie, Issue issue)
  {
    var alreadyGrabbed = (await radarr.GetDownloadQueue(movie.Id)).FirstOrDefault().AsMaybe();
    if (alreadyGrabbed.HasValue)
    {
      await overseerr.PostIssueComment(issue.Id, alreadyGrabbed.Value.AlreadyGrabbedMessage());
      await overseerr.PostIssueComment(issue.Id, issue.CloseMessage());
      await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
      return;
    }

    await radarr.GrabMovie(movie.Id);

    await timeOutProvider.AwaitDownloadQueueUpdated();

    var queue = await radarr.GetDownloadQueue(movie.Id);
    var grabbed = queue.FirstOrDefault().AsMaybe();
    await grabbed.Match(
      async file => await Grabbed(issue, file),
      async () => await overseerr.PostIssueComment(issue.Id, movie.NotGrabbedMessage())
    );
  }

  private async Task Grabbed(Issue issue, MovieDownload file)
  {
    await overseerr.PostIssueComment(issue.Id, file.GrabbedMessage());
    await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
  }

  private async Task DeleteMovieAsync(Issue issue, Movie movie, Maybe<MovieFile> file)
  {
    await file.Match(
      async f => await DeleteFile(issue, movie, f),
      async () => await overseerr.PostIssueComment(issue.Id, movie.NoFileToDeleteMessage()));
  }

  private async Task DeleteFile(Issue issue, Movie movie, MovieFile file)
  {
    await overseerr.PostIssueComment(issue.Id, movie.DeletionStartedMessage(file));
    await radarr.DeleteMovieFile(file.Id);
    await overseerr.PostIssueComment(issue.Id, file.DeletionFinishedMessage());
  }
}

internal static class TranslationExtensions
{
  internal static string CloseMessage(this Issue _) => Translations.CloseIssue;

  internal static string MovieNotFoundMessage(this Issue issue) =>
    string.Format(Translations.MovieNotFound, issue.Media.Id);

  internal static string AlreadyGrabbedMessage(this MovieDownload movie) =>
    string.Format(Translations.MovieAlreadyGrabbed, movie.Title, movie.EstimatedCompletionTime.ToLocalTime(),
      movie.EstimatedCompletionTime.ToLocalTime());

  internal static string GrabbedMessage(this MovieDownload movie) =>
    string.Format(Translations.MovieGrabbed, movie.Title, movie.GetReadableFileSize(),
      movie.EstimatedCompletionTime.ToLocalTime());

  internal static string NotGrabbedMessage(this Movie movie) =>
    string.Format(Translations.MovieNotGrabbed, movie.Title);

  internal static string NoFileToDeleteMessage(this Movie movie) =>
    string.Format(Translations.MovieAlreadyDeleted, movie.Title);

  internal static string DeletionStartedMessage(this Movie movie, MovieFile file) =>
    string.Format(Translations.MovieDeletionStart, movie.Title, file.GetReadableFileSize());

  internal static string DeletionFinishedMessage(this MovieFile file) =>
    string.Format(Translations.MovieDeletionFinished, file.GetReadableFileSize());
}