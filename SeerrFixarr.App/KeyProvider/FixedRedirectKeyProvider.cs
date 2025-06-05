using SeerrFixarr.App.Shared;

namespace SeerrFixarr.App.KeyProvider;

public class FixedRedirectKeyProvider(IEnumerable<string> keys, RedirectKeyManager redirectKeyManager)
    : BaseRedirectKeyProvider(redirectKeyManager)
{
    protected override IEnumerable<string> GetAvailableKeys()
    {
        return keys.Where(k => !_usedKeys.Contains(k)).ToList();
    }
}