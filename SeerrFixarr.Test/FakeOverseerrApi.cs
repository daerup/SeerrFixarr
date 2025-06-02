using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SeerrFixarr.Api.Overseerr;

namespace SeerrFixarr.Test;

internal class FakeOverseerrApi : IOverseerrApi
{
    internal readonly List<Issue> Issues = [];
    internal readonly Dictionary<int, UserLocalSettings> LocaleSettings = [];
    internal readonly Dictionary<int, List<Comment>> Comments =  [];

    public Task<UserLocalSettings> GetLocalSettingsOfUser(int userId)
    {
        if (LocaleSettings.TryGetValue(userId, out var settings)) return Task.FromResult(settings);
        throw new InvalidOperationException($"User with id {userId} not found");
    }

    public Task<Issues> GetIssues(int take = 100, int skip = 0, string sort = "added", string filter = "all")
    {
        var issuesChunk = Issues.Chunk(take).ToList();
        var pageInfo = new PageInfo
        {
            Page = skip + 1,
            Pages = issuesChunk.Count,
            Results = Issues.Count,
        };
        var issues = new Issues
        {
            PageInfo = pageInfo,
            Results = issuesChunk.Skip(skip).First().Select(i => new IssueId { Id = i.Id }).ToArray()
        };
        return Task.FromResult(issues);
    }

    public Task<Issue> GetIssue(int issueId)
    {
        return Task.FromResult(Issues.SingleWithApiException(i => i.Id == issueId));
    }

    public Task DeleteIssue(int issueId)
    {
        var issue = Issues.SingleWithApiException(i => i.Id == issueId);
        Issues.Remove(issue);
        return Task.CompletedTask;
    }

    public Task PostIssueComment(int issueId, Comment message)
    {
        if (!Comments.TryGetValue(issueId, out var value))
        {
            value = [];
            Comments[issueId] = value;
        }

        value.Add(message);
        return Task.CompletedTask;
    }

    public Task UpdateIssueStatus(int issueId, IssueStatus status)
    {
        var issue = Issues.SingleWithApiException(i => i.Id == issueId);
        var updatedIssue = issue with { Status = (int)status };
        Issues.Remove(issue);
        Issues.Add(updatedIssue);
        return Task.CompletedTask;
    }

    public void Setup(Issue issue)
    {
        if (Issues.Any(i => i.Id == issue.Id))
            throw new InvalidOperationException($"Issue with id {issue.Id} already exists");
        Issues.Add(issue);
    }
    
    public void Setup(UserWithSettings user)
    {
      LocaleSettings[user.User.Id] = user.Settings;
    }
}