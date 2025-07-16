using FakeItEasy;
using SeerrFixarr.App.KeyProvider;
using SeerrFixarr.App.Shared;
using Shouldly;

namespace SeerrFixarr.Test;

public class GuidRedirectKeyProviderTest
{
    private readonly RedirectKeyManager _redirectKeyManager = A.Fake<RedirectKeyManager>();

    [Fact]
    public void GetNextKey_ShouldReturnNewGuid()
    {
        // Arrange
        var provider = new GuidRedirectKeyProvider(_redirectKeyManager);

        // Act
        var key1 = provider.GetNext();
        var key2 = provider.GetNext();

        // Assert
        key2.HasValue.ShouldBe(true);
        key2.Value.Length.ShouldBe(5);
        key1.HasValue.ShouldBe(true);
        key1.Value.Length.ShouldBe(5);
        key1.ShouldNotBe(key2);
    }
}