using FakeItEasy;
using SeerrFixarr.App.KeyProvider;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Test.Infrastructure;
using Shouldly;

namespace SeerrFixarr.Test;

public class FixedRedirectKeyProviderTest
{
    private readonly RedirectKeyManager _redirectKeyManager = A.Fake<RedirectKeyManager>();

    [Fact]
    public void GetNextKey_ShouldReturnOneOfTheFixedKeys()
    {
        // Arrange
        var expectedKey = "fixed-redirect-key";
        var provider = new FixedRedirectKeyProvider([expectedKey], _redirectKeyManager);

        // Act
        var key = provider.GetNextKey();

        // Assert
        key.ShouldBe(expectedKey);
    }
    
    [Fact]
    public void GetNextKey_ShouldReturnDifferentKeysOnSubsequentCalls()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        var provider = new FixedRedirectKeyProvider(keys, _redirectKeyManager);

        // Act
        var firstKey = provider.GetNextKey();
        var secondKey = provider.GetNextKey();
        var thirdKey = provider.GetNextKey();

        // Assert
        firstKey.ShouldNotBeNull();
        firstKey.ShouldNotBe(secondKey);
        firstKey.ShouldNotBe(thirdKey);
        secondKey.ShouldNotBeNull();
        secondKey.ShouldNotBe(firstKey);
        secondKey.ShouldNotBe(thirdKey);
        thirdKey.ShouldNotBeNull();
        thirdKey.ShouldNotBe(firstKey);
        thirdKey.ShouldNotBe(secondKey);
    }
    
    [Fact]
    public void GetNextKey_ShouldReturnNullIfNoKeysAvailable()
    {
        // Arrange
        var provider = new FixedRedirectKeyProvider(Array.Empty<string>(), _redirectKeyManager);

        // Act
        var key = provider.GetNextKey();

        // Assert
        key.ShouldBeNull();
    }
    
    [Fact]
    public void GetNextKey_ShouldReturnNullWhenAllKeysUsed()
    {
        // Arrange
        var keys = new[] { "key1", "key2" };
        var provider = new FixedRedirectKeyProvider(keys, _redirectKeyManager);
        
        // Act
        provider.GetNextKey();
        provider.GetNextKey();
        var key = provider.GetNextKey();
        var anotherKey = provider.GetNextKey();

        // Assert
        key.ShouldBeNull();
        anotherKey.ShouldBeNull();
    }
    
    [Fact]
    public void GetNextKey_ReturnPreviouslyUsedKey_WhenItIsNoLongerUsed()
    {
        // Arrange
        var keys = new[] { "key1" };
        var provider = new FixedRedirectKeyProvider(keys, _redirectKeyManager);

        // Act;
        var firstKey = provider.GetNextKey();
        _redirectKeyManager.InvokeOnRedirectionKeyDestroyed(firstKey!);
        var secondKey = provider.GetNextKey();
        
        // Assert
        secondKey.ShouldBe(firstKey);
    }
    
    [Fact]
    public void GetNextKey_ReturnsNull_WhenAllKeysAlreadyUsed()
    {
        // Arrange
        var keys = new[] { "key1", "key2" };
        var provider = new FixedRedirectKeyProvider(keys, _redirectKeyManager);
        
        // Act
        _redirectKeyManager.InvokeOnRedirectionKeyCreated("key1");
        _redirectKeyManager.InvokeOnRedirectionKeyCreated("key2");
        var key = provider.GetNextKey();

        // Assert
        key.ShouldBeNull();
    }

    [Fact]
    public void OnRedirectionKeyCreated_WhenKeyAlreadyUsed_ShouldNotThrow()
    {
        // Arrange
        var keys = new[] { "key1" };
        var provider = new FixedRedirectKeyProvider(keys, _redirectKeyManager);
        var key = provider.GetNextKey();
        _redirectKeyManager.InvokeOnRedirectionKeyCreated(key!);
        
        // Act
        var act = () => _redirectKeyManager.InvokeOnRedirectionKeyCreated(key!);
        
        // Assert
        act.ShouldNotThrow();
    }
    
    [Fact]
    public void OnRedirectionKeyDestroyed_WhenKeyNotUsed_ShouldNotThrow()
    {
        // Arrange
        var keys = new[] { "key1" };
        var provider = new FixedRedirectKeyProvider(keys, _redirectKeyManager);
        
        // Act
        var act = () => _redirectKeyManager.InvokeOnRedirectionKeyDestroyed("key1");
        
        // Assert
        act.ShouldNotThrow();
    }
    
    [Fact]
    public void OnRedirectionKeyDestroyed_WhenKeyUsed_ShouldRemoveFromUsedKeys()
    {
        // Arrange
        var keys = new[] { "key1" };
        var provider = new FixedRedirectKeyProvider(keys, _redirectKeyManager);
        var key = provider.GetNextKey();
        
        // Act
        _redirectKeyManager.InvokeOnRedirectionKeyDestroyed(key!);
        var nextKey = provider.GetNextKey();
        
        // Assert
        nextKey.ShouldBe(key);
    }

}