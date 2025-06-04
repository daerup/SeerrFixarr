using Microsoft.AspNetCore.WebUtilities;

namespace SeerrFixarr.App.Shared;

internal record RedirectionTarget(string Token, string Url);

public class UrlRedirectionCreator
{
    private Dictionary<string, RedirectionTarget> _redirections = new();

    public UrlRedirectionCreator(TokenCreator tokenCreator)
    {
        tokenCreator.OnTokenInvalidated += RemoveRevokedRedirections;
    }

    private void RemoveRevokedRedirections(string token)
    {
        _redirections = _redirections
            .Where(kv => kv.Value.Token != token)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public void AddRedirection(string alias, string token)
    {
        if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Key and URL must not be null or empty.");
        }

        Dictionary<string, string?> queryParams = new() { { Constants.TokenQueryParameterName, token } };
        var selectUrl = QueryHelpers.AddQueryString("/select", queryParams);
        _redirections[alias] = new RedirectionTarget(token, selectUrl);
    }

    public string GetTargetOfRedirection(string alias)
    {
        if (!_redirections.TryGetValue(alias, out var redirection))
        {
            throw new KeyNotFoundException($"No redirection found for alias '{alias}'.");
        }

        return redirection.Url;
    }
}