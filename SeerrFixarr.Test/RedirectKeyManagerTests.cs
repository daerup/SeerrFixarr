using FakeItEasy;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Test.Infrastructure;
using Shouldly;

namespace SeerrFixarr.Test;

public class RedirectKeyManagerTests
{
    private RedirectKeyManager _testee;
    private TokenCreator _tokenCreator;

    public RedirectKeyManagerTests()
    {
        _tokenCreator = A.Fake<TokenCreator>();
        _testee = new RedirectKeyManager(_tokenCreator);
    }

    [Fact]
    public void CreateRedirection_ShouldThrowArgumentException_WhenKeyIsNull()
    {
        // Arrange
        string key = null!;

        // Act
        var act = () => _testee.AddRedirection(key, A.Dummy<string>());

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void CreateRedirection_ShouldThrowArgumentException_WhenTokenIsNull()
    {
        // Arrange
        string token = null!;

        // Act
        var act = () => _testee.AddRedirection(A.Dummy<string>(), token);

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData("key1")]
    [InlineData("")]
    public void GetRedirection_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist(string key)
    {
        // Act
        var act = () => _testee.GetRedirectionTargetFromKey(key);

        // Assert
        act.ShouldThrow<KeyNotFoundException>();
    }

    [Fact]
    public void AddRedirection_ShouldAddRedirection_WhenValidKeyAndTokenProvided()
    {
        // Arrange
        var key = "key1";
        var token = "secret jwt token";
        var tokenEncoded = Uri.EscapeDataString(token);
        var expectedUrl = $"/select?token={tokenEncoded}";

        // Act
        _testee.AddRedirection(key, token);
        var redirection = _testee.GetRedirectionTargetFromKey(key);

        // Assert
        redirection.ShouldNotBeNull();
        redirection.ShouldBe(expectedUrl);
    }

    [Fact]
    public void WhenTokenGetsRevoked_RedirectionShouldBeRemoved()
    {
        // Arrange
        var key = "key1";
        var token = "secret jwt token";
        _testee.AddRedirection(key, token);

        // Act
        _tokenCreator.InvokeOnTokenRevoked(token);
        var act = () =>  _testee.GetRedirectionTargetFromKey(key);

        // Assert
        act.ShouldThrow<KeyNotFoundException>();
    }
    
    [Fact]
    public void WhenOtherTokenGetsRevoked_RedirectionShouldBeNotRemoved()
    {
        // Arrange
        var key = "key1";
        var token = "secret jwt token";
        var otherToken = "other token";
        _testee.AddRedirection(key, token);

        // Act
        _tokenCreator.InvokeOnTokenRevoked(otherToken);
        var act = () => _testee.GetRedirectionTargetFromKey(key);

        // Assert
        act.ShouldNotThrow();
    }
}