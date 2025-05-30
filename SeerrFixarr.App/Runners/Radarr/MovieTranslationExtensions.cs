using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;

namespace SeerrFixarr.App.Runners.Radarr;

internal static class MovieTranslationExtensions
{
  internal static string MovieNotFoundMessage(this Issue issue) =>
    string.Format(Translations.MovieNotFound, issue.Media.Id);

  internal static string AlreadyGrabbedMessage(this MovieDownload movie) =>
    string.Format(Translations.MovieAlreadyGrabbed, movie.Title, movie.EstimatedCompletionTime.ToLocalTime());

  internal static string GrabbedMessage(this MovieDownload movie) =>
    string.Format(Translations.MovieGrabbed, movie.Title, movie.GetReadableFileSize(),
      movie.EstimatedCompletionTime.ToLocalTime());

  internal static string NotGrabbedMessage(this Movie movie) =>
    string.Format(Translations.ItemNotGrabbed, movie.Title);

  internal static string NoFileToDeleteMessage(this Movie movie) =>
    string.Format(Translations.MovieFileNotFound, movie.Title);

  internal static string DeletionStartedMessage(this Movie movie, MovieFile file) =>
    string.Format(Translations.MovieDeletionStart, movie.Title, file.GetReadableFileSize());

  internal static string DeletionFinishedMessage(this MovieFile file) =>
    string.Format(Translations.MovieDeletionFinished, file.GetReadableFileSize());
  
}