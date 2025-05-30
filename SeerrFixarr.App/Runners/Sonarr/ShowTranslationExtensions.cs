using SeerrFixarr.Api.Sonarr;

namespace SeerrFixarr.App.Runners.Sonarr;

internal static class ShowTranslationExtensions
{
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