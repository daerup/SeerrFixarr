using System.Collections.Concurrent;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Shared.Settings;

namespace SeerrFixarr.App.KeyProvider;

public class RedirectKeyProviderFactory(
    SeerrFixarrSettings settings,
    GuidRedirectKeyProvider guidKeyProvider,
    RedirectKeyManager redirectKeyManager)
{
    private readonly ConcurrentDictionary<string, IRedirectKeyProvider> _cache = new();

    public IRedirectKeyProvider GetKeyProviderForIdentifier(string username)
    {
        return settings.UserRedirectKeyPool.TryGetValue(username, out var pool)
            ? _cache.GetOrAdd(username, _ => new FixedRedirectKeyProvider(pool.ToList(), redirectKeyManager))
            : guidKeyProvider;
    }
}