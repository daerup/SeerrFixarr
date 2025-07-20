using FakeItEasy;
using SeerrFixarr.App.KeyProvider;
using Shouldly;

namespace SeerrFixarr.Test;

public class RedirectKeyFactoryTests
{
    private const string KnownIdentifier = "test-identifier";
    private const string UnknownIdentifier = "test-identifier-that-does-not-exist";
    private readonly RedirectKeyFactory _testee;
    private readonly GuidRedirectKeyProvider _fakeGuidProvider;
    private readonly FixedRedirectKeyProvider _fakeFixedProvider;

    public RedirectKeyFactoryTests()
    {
        var fakeProviderCache = A.Fake<FixedRedirectKeyProviderCache>();
        _fakeGuidProvider = A.Fake<GuidRedirectKeyProvider>();
        _fakeFixedProvider = A.Fake<FixedRedirectKeyProvider>();
        A.CallTo(() => fakeProviderCache.GetKeyProviderForIdentifier(KnownIdentifier)).Returns(_fakeFixedProvider);
        _testee = new RedirectKeyFactory(fakeProviderCache, _fakeGuidProvider);
    }

    [Fact]
    public void GetKeyForIdentifier_ShouldReturnNextKeyOfFixedProvider_WhenFixedProviderForIdentifierExists()
    {
        // Arrange
        A.CallTo(() => _fakeFixedProvider.GetNext()).Returns("12345");

        // Act
        var key = _testee.GetKeyForIdentifier(KnownIdentifier);

        // Assert
        key.ShouldBe("12345");
        A.CallTo(() => _fakeFixedProvider.GetNext()).MustHaveHappened();
        A.CallTo(() => _fakeGuidProvider.GetNext()).MustNotHaveHappened();
    }

    [Fact]
    public void GetKeyForIdentifier_ShouldReturnNextKeyOfGuidProvider_WhenNoFixedProviderExists()
    {
        // Arrange
        A.CallTo(() => _fakeGuidProvider.GetNext()).Returns("67890");

        // Act
        var key = _testee.GetKeyForIdentifier(UnknownIdentifier);

        // Assert
        key.ShouldBe("67890");
        A.CallTo(() => _fakeFixedProvider.GetNext()).MustNotHaveHappened();
        A.CallTo(() => _fakeGuidProvider.GetNext()).MustHaveHappened();
    }
    
    [Fact]
    public void GetKeyForIdentifier_ShouldReturnNextKeyOfGuidProvider_WhenFixedProviderReturnsNull()
    {
        // Arrange
        A.CallTo(() => _fakeFixedProvider.GetNext()).Returns((string?)null);
        A.CallTo(() => _fakeGuidProvider.GetNext()).Returns("67890");

        // Act
        var key = _testee.GetKeyForIdentifier(KnownIdentifier);

        // Assert
        key.ShouldBe("67890");
        A.CallTo(() => _fakeFixedProvider.GetNext()).MustHaveHappened();
        A.CallTo(() => _fakeGuidProvider.GetNext()).MustHaveHappened();
    }
}