using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;

namespace SeerrFixarr.App.Runners.Radarr;

internal static class TranslationExtensions
{
  internal static string CloseMessage(this Issue _) => Translations.CloseIssue;

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
  
  internal static string WholeShowFaultyMessage() => Translations.WholeShowFaulty;
  internal static string WholeSeasonFaultyMessage() => Translations.WholeSeasonFaulty;
  
  internal static string EpisodeNotFoundMessage(string identifier) =>
    string.Format(Translations.EpisodeNotFound, identifier);

  internal static string AlreadyGrabbedMessage(this EpisodeDownload episode, string identifier) =>
    string.Format(Translations.EpisodeAlreadyGrabbed, identifier, episode.EstimatedCompletionTime.ToLocalTime());

  internal static string GrabbedMessage(this EpisodeDownload episode) =>
    string.Format(Translations.EpisodeGrabbed, episode.Title, episode.GetReadableFileSize(),
      episode.EstimatedCompletionTime.ToLocalTime());

  internal static string EpisodeNotGrabbedMessage(string identifier) =>
    string.Format(Translations.ItemNotGrabbed, identifier);
  
  internal static string NoEpisodeFileToDeleteMessage(string identifier) =>
    string.Format(Translations.EpisodeFileNotFound, identifier);

  internal static string DeletionStartedMessage(this EpisodeFile file, string identifier) =>
  string.Format(Translations.EpisodeDeletionStart, identifier, file.GetReadableFileSize());

  internal static string DeletionFinishedMessage(this EpisodeFile file, string identifier) =>
    string.Format(Translations.EpisodeDeletionFinished, identifier, file.GetReadableFileSize());
}