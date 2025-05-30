using SeerrFixarr.Api.Overseerr;

namespace SeerrFixarr.App.Runners;

internal static class IssueTranslationExtensions
{
    internal static string CloseMessage(this Issue _) => Translations.CloseIssue;
}