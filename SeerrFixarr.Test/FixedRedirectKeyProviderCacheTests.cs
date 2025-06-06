using FakeItEasy;
using SeerrFixarr.App.KeyProvider;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Shared.Settings;
using Shouldly;

namespace SeerrFixarr.Test;

public class FixedRedirectKeyProviderCacheTests
{
    private readonly RedirectKeyManager _redirectKeyManager = A.Fake<RedirectKeyManager>();

    [Fact]
    public void GetKeyProviderForUser_ShouldReturnFixedProvider_WhenUserHasPool()
    {
        // Arrange
        var settings = new SeerrFixarrSettings { UserRedirectKeyPool = new() { ["user1"] = ["alpha", "beta"] } };
        var testee = new FixedRedirectKeyProviderCache(settings, _redirectKeyManager);
        
        // Act
        var provider = testee.GetKeyProviderForIdentifier("user1");

        // Assert
        provider.HasValue.ShouldBeTrue();
        provider.Value.ShouldBeOfType<FixedRedirectKeyProvider>();
    }

    [Fact]
    public void GetKeyProviderForUser_ShouldCacheFixedProvider_ForSameUser()
    {
        // Arrange
        var settings = new SeerrFixarrSettings { UserRedirectKeyPool = new() { ["user2"] = ["1", "2"] } };
        var testee = new FixedRedirectKeyProviderCache(settings, _redirectKeyManager);

        // Act
        var first = testee.GetKeyProviderForIdentifier("user2");
        var second = testee.GetKeyProviderForIdentifier("user2");

        // Assert
        first.ShouldBe(second);

    }

    [Fact]
    public void GetKeyProviderForUser_ShouldReturnNone_WhenUserHasNoPool()
    {
        // Arrange
        var settings = new SeerrFixarrSettings { UserRedirectKeyPool = [] };
        var testee = new FixedRedirectKeyProviderCache(settings, _redirectKeyManager);

        // Act
        var provider = testee.GetKeyProviderForIdentifier("user1");

        // Assert
        provider.HasValue.ShouldBeFalse();
    }
}