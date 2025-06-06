using System.Diagnostics;
using CSharpFunctionalExtensions;

namespace SeerrFixarr.App.KeyProvider;

public class RedirectKeyFactory(FixedRedirectKeyProviderCache cache, GuidRedirectKeyProvider guidKeyProvider)
{
    public string GetKeyForIdentifier(string id)
    {
        return cache.GetKeyProviderForIdentifier(id).Match(
            provider => provider.GetNext(),
            guidKeyProvider.GetNext
        ) ?? throw new UnreachableException();
    }
}