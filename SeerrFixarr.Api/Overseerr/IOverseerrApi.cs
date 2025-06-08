using Refit;

namespace SeerrFixarr.Api.Overseerr;

public interface IOverseerrApi
{
    [Get("/user/{id}/settings/main")]
    Task<UserLocalSettings> GetLocalSettingsOfUser([AliasAs("id")] int userId);
    
    [Get("/issue")]
    Task<Issues> GetIssues(int take = 100, int skip = 0, string sort = "added", string filter = "all");
    
    [Get("/issue/{issueId}")]
    Task<Issue> GetIssue(int issueId);
    
    [Delete("/issue/{issueId}")]
    Task DeleteIssue(int issueId);
    
    [Post("/issue/{issueId}/comment")]
    Task PostIssueComment(int issueId, [Body] Comment comment);
    
    [Post("/issue/{issueId}/{status}")]
    Task UpdateIssueStatus(int issueId, IssueStatus status);
}