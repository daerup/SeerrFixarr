using System.Collections.Concurrent;
using Microsoft.AspNetCore.WebUtilities;

namespace SeerrFixarr.App.Shared;

internal record RedirectionTarget(string Token, string Url);

public class RedirectKeyManager
{
    private readonly ConcurrentDictionary<string, RedirectionTarget> _redirections = new();

    public RedirectKeyManager(TokenCreator tokenCreator)
    {
        tokenCreator.OnTokenRevoked += RemoveRevokedRedirections;
    }

    public event Action<string> OnRedirectionKeyCreated = delegate { };
    public event Action<string> OnRedirectionKeyDestroyed = delegate { };

    public void AddRedirection(string key, string token)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Key and URL must not be null or empty.");
        }

        Dictionary<string, string?> queryParams = new() { { Constants.TokenQueryParameterName, token } };
        var selectUrl = QueryHelpers.AddQueryString("/select", queryParams);
        _redirections[key] = new RedirectionTarget(token, selectUrl);
        OnRedirectionKeyCreated(key);
    }

    public string GetRedirectionTargetFromKey(string key)
    {
        if (!_redirections.TryGetValue(key, out var redirection))
        {
            throw new KeyNotFoundException($"No redirection found for alias '{key}'.");
        }

        return redirection.Url;
    }
    
    private void RemoveRevokedRedirections(string token)
    {
        foreach (var kvp in _redirections)
        {
            if (kvp.Value.Token != token) continue;
            if (_redirections.TryRemove(kvp.Key, out _))
            {
                OnRedirectionKeyDestroyed(kvp.Key);
            }
        }
    }
}