using SeerrFixarr.Shared.Settings;

namespace SeerrFixarr.App.Extensions;

internal class UserRedirectKeyPoolConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new UserRedirectKeyPoolConfigurationProvider(nameof(SeerrFixarrSettings.UserRedirectKeyPool));
    }
}