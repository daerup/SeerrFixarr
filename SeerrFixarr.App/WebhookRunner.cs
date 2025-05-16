using System.Dynamic;
using System.Text;
using CSharpFunctionalExtensions;
using Refit;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;
using UnitsNet;

namespace SeerrFixarr.App;

public class WebhookRunner(IOverseerrApi overseerr, IRadarrApi radarr, ISonarrApi sonarr)
{
    public async Task RunAsync(dynamic body)
    {
        var issue = await overseerr.GetIssue(26);
        var (movie, moviefile) = await GetMovieFileFileFromIssue(issue);

        await DeleteMovieAsync(issue, movie, moviefile);
        await Task.Delay(TimeSpan.FromSeconds(5));
        await GrabMovie(movie, issue);
    }

    private async Task GrabMovie(Movie movie, Issue issue)
    {
        var alreadyGrabbed = (await radarr.GetDownloadQueue(movie.Id)).FirstOrDefault().AsMaybe();
        if (alreadyGrabbed.HasValue)
        {
            await overseerr.PostIssueComment(issue.Id, @$" ⬇️ Already grabbed file '{alreadyGrabbed.Value.Title}. 🕒 {alreadyGrabbed.Value.EstimatedCompletionTime.ToLocalTime()}'");
            await overseerr.PostIssueComment(issue.Id, @$" This issue will be closed. Reopen if the problem persists.");
            await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
            return;
        }
        
        var queue = await radarr.GetDownloadQueue(movie.Id);
        var grabbed = queue.FirstOrDefault().AsMaybe();
        
        await radarr.GrabMovie(movie.Id);
        
        await grabbed.Match(async f =>
            {
                var fileSize = GetFileSize(f.Size);
                var commentBuilder = new StringBuilder();
                commentBuilder.Append(@$" ⬇️ Grabbed file '{f.Title}'");
                commentBuilder.Append(@$" 💾 {fileSize}");
                commentBuilder.Append(@$" 🕒 {f.EstimatedCompletionTime.ToLocalTime()}");
                await overseerr.PostIssueComment(issue.Id, commentBuilder.ToString());
            },
            async () =>
            {
                await overseerr.PostIssueComment(issue.Id, @$" 🥺 Could not grab file for '{movie.Title}'");
            });
    }

    private async Task DeleteMovieAsync(Issue issue, Movie movie, Maybe<MovieFile> moviefile)
    {
        await moviefile.Match(async f =>
            {
                var fileSize = GetFileSize(f.Size);
                await overseerr.PostIssueComment(issue.Id, @$"🗑️ Deleting file of '{movie.Title}' ({fileSize})");
                await radarr.DeleteMovieFile(f.Id);
                await overseerr.PostIssueComment(issue.Id, @$"✅ Successfully deleted movie file ({fileSize})");
                await overseerr.UpdateIssueStatus(issue.Id, IssueStatus.Resolved);
            },
            async () =>
            {
                await overseerr.PostIssueComment(issue.Id, @$"⏩ Movie file not found for '{movie.Title}', skipping deletion");
            });
    }

    private static string GetFileSize(long size)
    {
        var fileSize = Information.FromBytes(size);
        return $"{fileSize.Gigabytes:F2} GB";
    }

    private async Task<(Movie, Maybe<MovieFile>)> GetMovieFileFileFromIssue(Issue issue)
    {
        try
        {
            var movie = await radarr.GetMovie(issue.Media.Id);
            return (movie, movie.MovieFile);
        }
        catch (ApiException)
        {
            var movie = (await radarr.GetMovies(issue.Media.TmdbId!.Value)).Single();
            return (movie, movie.MovieFile);
        }
    }
}