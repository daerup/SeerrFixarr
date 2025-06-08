using SeerrFixarr.Api.Overseerr;
using Serilog;

namespace SeerrFixarr.App;

public class OverseerrApiLoggingInterceptor(IOverseerrApi inner) : IOverseerrApi
{
    public Task<UserLocalSettings> GetLocalSettingsOfUser(int userId) => inner.GetLocalSettingsOfUser(userId);
    public Task<Issues> GetIssues(int take = 100, int skip = 0, string sort = "added", string filter = "all") => inner.GetIssues(take, skip, sort, filter);
    public Task<Issue> GetIssue(int issueId) => inner.GetIssue(issueId);
    public Task DeleteIssue(int issueId) => inner.DeleteIssue(issueId);

    public Task UpdateIssueStatus(int issueId, IssueStatus status)
    {
        Log.Information("Updating Issue #{IssueId} Status to {Status}", issueId, status);
        return inner.UpdateIssueStatus(issueId, status);
    }

    public Task PostIssueComment(int issueId, Comment comment)
    {
        Log.Information("Posting Comment on Issue #{IssueId}: {Message}", issueId, comment.Message);
        return inner.PostIssueComment(issueId, comment);
    }
}