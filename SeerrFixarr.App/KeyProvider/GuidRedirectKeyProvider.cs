using SeerrFixarr.App.Shared;

namespace SeerrFixarr.App.KeyProvider;

public class GuidRedirectKeyProvider(RedirectKeyManager redirectKeyManager) : BaseRedirectKeyProvider(redirectKeyManager)
{
    protected override IEnumerable<string> GetAvailableKeys()
    {
        var guid = Guid.CreateVersion7();
        var segments = guid.ToString().Split('-');
        var initialCharOfSegments = segments.Select(s => s[0].ToString());
        var generatedKey = string.Join("", initialCharOfSegments);
        return [generatedKey];
    }
}