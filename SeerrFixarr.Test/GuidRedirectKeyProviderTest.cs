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
        var key = provider.GetNext();

        // Assert
        key.ShouldNotBeNull();
        key.Length.ShouldBe(5);
    }
}