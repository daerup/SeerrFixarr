using FakeItEasy;
using SeerrFixarr.App.KeyProvider;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Test.Infrastructure;
using Shouldly;

namespace SeerrFixarr.Test;

public class FixedRedirectKeyProviderTest
{
    private readonly RedirectKeyManager _redirectKeyManager = new RedirectKeyManager(A.Dummy<TokenCreator>());

    [Fact]
    public void GetNextKey_ShouldReturnOneOfTheFixedKeys()
    {
        // Arrange
        var expectedKey = "fixed-redirect-key";
        var provider = new FixedRedirectKeyProvider([expectedKey], _redirectKeyManager);

        // Act
        var key = provider.GetNext();

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
        var firstKey = provider.GetNext();
        var secondKey = provider.GetNext();
        var thirdKey = provider.GetNext();

        // Assert
        firstKey.HasValue.ShouldBe(true);
        firstKey.ShouldNotBe(secondKey);
        firstKey.ShouldNotBe(thirdKey);
        secondKey.HasValue.ShouldBe(true);
        secondKey.ShouldNotBe(firstKey);
        secondKey.ShouldNotBe(thirdKey);
        thirdKey.HasValue.ShouldBe(true);
        thirdKey.ShouldNotBe(firstKey);
        thirdKey.ShouldNotBe(secondKey);
    }
    
    [Fact]
    public void GetNextKey_ShouldReturnNullIfNoKeysAvailable()
    {
        // Arrange
        var provider = new FixedRedirectKeyProvider([], _redirectKeyManager);

        // Act
        var key = provider.GetNext();

        // Assert
        key.HasNoValue.ShouldBe(true);
    }
    
    [Fact]
    public void GetNextKey_ShouldReturnNullWhenAllKeysUsed()
    {
        // Arrange
        var keys = new[] { "key1", "key2" };
        var provider = new FixedRedirectKeyProvider(keys, _redirectKeyManager);
        
        // Act
        provider.GetNext();
        provider.GetNext();
        var key = provider.GetNext();
        var anotherKey = provider.GetNext();

        // Assert
        key.HasNoValue.ShouldBe(true);
        anotherKey.HasNoValue.ShouldBe(true);
    }
    
    [Fact]
    public void GetNextKey_ReturnPreviouslyUsedKey_WhenItIsNoLongerUsed()
    {
        // Arrange
        var keys = new[] { "key1" };
        var provider = new FixedRedirectKeyProvider(keys, _redirectKeyManager);

        // Act;
        var firstKey = provider.GetNext();
        _redirectKeyManager.InvokeOnRedirectionKeyDestroyed(firstKey.Value);
        var secondKey = provider.GetNext();
        
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
        var key = provider.GetNext();

        // Assert
        key.HasNoValue.ShouldBe(true);
    }

    [Fact]
    public void OnRedirectionKeyCreated_WhenKeyAlreadyUsed_ShouldNotThrow()
    {
        // Arrange
        var keys = new[] { "key1" };
        var provider = new FixedRedirectKeyProvider(keys, _redirectKeyManager);
        var key = provider.GetNext();
        _redirectKeyManager.InvokeOnRedirectionKeyCreated(key.Value);
        
        // Act
        var act = () => _redirectKeyManager.InvokeOnRedirectionKeyCreated(key.Value);
        
        // Assert
        act.ShouldNotThrow();
    }
    
    [Fact]
    public void OnRedirectionKeyDestroyed_WhenKeyNotUsed_ShouldNotThrow()
    {
        // Arrange
        var keys = new[] { "key1" };
        _ = new FixedRedirectKeyProvider(keys, _redirectKeyManager);
        
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
        var key = provider.GetNext();
        
        // Act
        _redirectKeyManager.InvokeOnRedirectionKeyDestroyed(key.Value);
        var nextKey = provider.GetNext();
        
        // Assert
        nextKey.ShouldBe(key);
    }

}