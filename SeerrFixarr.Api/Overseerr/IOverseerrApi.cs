using Refit;

namespace SeerrFixarr.Api.Overseerr;

public interface IOverseerrApi
{
    [Get("/issue/{issueId}")]
    Task<Issue> GetIssue(int issueId);
    
    [Post("/issue/{issueId}/comment")]
    Task PostIssueComment(int issueId, [Body] Comment message);
    
    [Post("/issue/{issueId}/{status}")]
    Task UpdateIssueStatus(int issueId, IssueStatus status);
}