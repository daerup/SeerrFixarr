using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using SeerrFixarr.App.KeyProvider;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Shared.Settings;
using Shouldly;

namespace SeerrFixarr.Test;

public class RedirectKeyProviderFactoryTests
{
    private readonly RedirectKeyManager _redirectKeyManager = A.Fake<RedirectKeyManager>();
    private readonly GuidRedirectKeyProvider _guidProvider = A.Fake<GuidRedirectKeyProvider>();

    [Fact]
    public void GetKeyProviderForUser_ShouldReturnFixedProvider_WhenUserHasPool()
    {
        // Arrange
        var settings = new SeerrFixarrSettings { UserRedirectKeyPool = new() { ["user1"] = ["alpha", "beta"] } };
        var testee = new RedirectKeyProviderFactory(settings, _guidProvider, _redirectKeyManager);
        
        // Act
        var provider = testee.GetKeyProviderForUser("user1");

        // Assert
        provider.ShouldBeOfType<FixedRedirectKeyProvider>();
    }

    [Fact]
    public void GetKeyProviderForUser_ShouldReturnGuidProvider_WhenUserHasNoPool()
    {
        // Arrange
        var settings = new SeerrFixarrSettings { UserRedirectKeyPool = [] };
        var testee = new RedirectKeyProviderFactory(settings, _guidProvider, _redirectKeyManager);
        
        // Act
        var provider = testee.GetKeyProviderForUser("missing_user");

        // Assert
        provider.ShouldBe(_guidProvider);
    }

    [Fact]
    public void GetKeyProviderForUser_ShouldCacheFixedProvider_ForSameUser()
    {
        // Arrange
        var settings = new SeerrFixarrSettings { UserRedirectKeyPool = new() { ["user2"] = ["1", "2"] } };
        var testee = new RedirectKeyProviderFactory(settings, _guidProvider, _redirectKeyManager);
        
        // Act
        var first = testee.GetKeyProviderForUser("user2");
        var second = testee.GetKeyProviderForUser("user2");

        // Assert
        first.ShouldBe(second);
    }
}