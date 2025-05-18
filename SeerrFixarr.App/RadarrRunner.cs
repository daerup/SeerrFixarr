using CSharpFunctionalExtensions;
using Refit;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;

namespace SeerrFixarr.App;

public class RadarrRunner(
    IOverseerrApi overseerr,
    IRadarrApi radarr,
    ITimeOutProvider timeOutProvider,
    FileSizeFormatter fileSizeFormatter)
{
    public async Task HandleMovieIssue(Issue issue)
    {
        var (movie, moviefile) = await GetMovieFileFromIssue(issue);
        await DeleteMovieAsync(issue, movie, moviefile);
        await timeOutProvider.AwaitFileDeletion();
        await GrabMovie(movie, issue);
    }

    private async Task<(Movie, Maybe<MovieFile>)> GetMovieFileFromIssue(Issue issue)
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
                await overseerr.PostIssueComment(issue.Id, @$"🔎 Could not find movie with id '{issue.Media.Id}'");
                throw;
            }
        }
    }

    private async Task GrabMovie(Movie movie, Issue issue)
    {
        var alreadyGrabbed = (await radarr.GetDownloadQueue(movie.Id)).FirstOrDefault().AsMaybe();
        if (alreadyGrabbed.HasValue)
        {
            await overseerr.PostIssueComment(issue.Id,
                @$"⬇️ Already grabbed file '{alreadyGrabbed.Value.Title}. 🕒 {alreadyGrabbed.Value.EstimatedCompletionTime.ToLocalTime()}'");
            await overseerr.PostIssueComment(issue.Id,
                @"✅️ This issue will be closed. Reopen if the problem persists.");
            await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
            return;
        }

        await radarr.GrabMovie(movie.Id);

        await timeOutProvider.AwaitDownloadQueueUpdated();

        var queue = await radarr.GetDownloadQueue(movie.Id);
        var grabbed = queue.FirstOrDefault().AsMaybe();
        await grabbed.Match(
            async file => await Grabbed(issue, file),
            async () => await overseerr.PostIssueComment(issue.Id, @$"🥺 Could not grab file for '{movie.Title}'")
        );
    }

    private async Task Grabbed(Issue issue, MovieDownload file)
    {
        var fileSize = fileSizeFormatter.GetFileSize(file.Size);
        var comment = @$"⬇️ Grabbed file '{file.Title}' 💾 {fileSize} 🕒 {file.EstimatedCompletionTime.ToLocalTime()}";
        await overseerr.PostIssueComment(issue.Id, comment);
        await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
    }

    private async Task DeleteMovieAsync(Issue issue, Movie movie, Maybe<MovieFile> moviefile)
    {
        await moviefile.Match(
            async f => await DeleteFile(issue, movie, f),
            async () => await overseerr.PostIssueComment(issue.Id,
                @$"⏩ Movie file not found for '{movie.Title}', skipping deletion"));
    }

    private async Task DeleteFile(Issue issue, Movie movie, MovieFile file)
    {
        var fileSize = fileSizeFormatter.GetFileSize(file.Size);
        await overseerr.PostIssueComment(issue.Id, @$"🗑️ Deleting file of '{movie.Title}' ({fileSize})");
        await radarr.DeleteMovieFile(file.Id);
        await overseerr.PostIssueComment(issue.Id, @$"✅ Successfully deleted movie file ({fileSize})");
    }
}