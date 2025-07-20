using System.Collections.Concurrent;
using CSharpFunctionalExtensions;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Shared.Settings;

namespace SeerrFixarr.App.KeyProvider;

public class FixedRedirectKeyProviderCache(SeerrFixarrSettings settings, RedirectKeyManager redirectKeyManager)
{
    private readonly ConcurrentDictionary<string, IRedirectKeyProvider> _cache = new();

    public Maybe<IRedirectKeyProvider> GetKeyProviderForIdentifier(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !settings.UserRedirectKeyPool.TryGetValue(id, out var pool))
        {
            return Maybe<IRedirectKeyProvider>.None;
        }

        var provider = _cache.GetOrAdd(id, _ => new FixedRedirectKeyProvider(pool.ToList(), redirectKeyManager));
        return Maybe.From(provider);
    }
}