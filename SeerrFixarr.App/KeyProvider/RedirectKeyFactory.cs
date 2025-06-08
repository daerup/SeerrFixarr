using System.Diagnostics;
using CSharpFunctionalExtensions;
using Serilog;

namespace SeerrFixarr.App.KeyProvider;

public class RedirectKeyFactory(FixedRedirectKeyProviderCache cache, GuidRedirectKeyProvider guidKeyProvider)
{
    public string GetKeyForIdentifier(string id)
    {
        var providerForIdentifier = cache.GetKeyProviderForIdentifier(id);
        providerForIdentifier.Execute(provider =>
            Log.Information("Found key provider {provider} for identifier {Identifier}", provider, id));
        providerForIdentifier.ExecuteNoValue(() =>
            Log.Information("No key provider found for identifier {Identifier}", id));

        return providerForIdentifier.Or(guidKeyProvider)
            .SelectMany(p => p.GetNext()).Or(guidKeyProvider.GetNext())
            .GetValueOrThrow(new UnreachableException());
    }
}