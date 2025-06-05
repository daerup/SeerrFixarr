using SeerrFixarr.App.Shared;

namespace SeerrFixarr.App.KeyProvider;

public abstract class BaseRedirectKeyProvider : IRedirectKeyProvider
{
    protected readonly HashSet<string?> _usedKeys = [];
    private readonly Random _random = new();

    protected BaseRedirectKeyProvider(RedirectKeyManager redirectKeyManager)
    {
        redirectKeyManager.OnRedirectionKeyCreated += MarkKeyAsUsed;
        redirectKeyManager.OnRedirectionKeyDestroyed += ReleaseKey;
    }
    
    public string? GetNextKey()
    {
        var unusedKeys = GetAvailableKeys().ToList();
        if (unusedKeys.Count == 0)
        {
            return null;
        }

        var index = _random.Next(unusedKeys.Count);
        var key = unusedKeys[index];
        _usedKeys.Add(key);
        return key;
    }

    protected abstract IEnumerable<string> GetAvailableKeys();

    private void MarkKeyAsUsed(string? key)
    {
        _usedKeys.Add(key);
    }

    private void ReleaseKey(string? key)
    {
        _usedKeys.Remove(key);
    }
}