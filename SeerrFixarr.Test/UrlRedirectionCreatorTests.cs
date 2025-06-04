using System.Reflection;
using FakeItEasy;
using SeerrFixarr.App.Shared;
using Shouldly;

namespace SeerrFixarr.Test;

public class UrlRedirectionCreatorTests
{
    private UrlRedirectionCreator _testee;
    private TokenCreator _tokenCreator;

    public UrlRedirectionCreatorTests()
    {
        _tokenCreator = A.Fake<TokenCreator>();
        _testee = new UrlRedirectionCreator(_tokenCreator);
    }

    [Fact]
    public void CreateRedirection_ShouldThrowArgumentNullException_WhenAliasIsNull()
    {
        // Arrange
        string alias = null!;

        // Act
        var act = () => _testee.AddRedirection(alias, A.Dummy<string>());

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateRedirection_ShouldThrowArgumentNullException_WhenTokenIsNull()
    {
        // Arrange
        string token = null!;

        // Act
        var act = () => _testee.AddRedirection(A.Dummy<string>(), token);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("alias1")]
    [InlineData("")]
    public void GetRedirection_ShouldThrowKeyNotFoundException_WhenAliasDoesNotExist(string alias)
    {
        // Act
        var act = () => _testee.GetTargetOfRedirection(alias);

        // Assert
        Should.Throw<KeyNotFoundException>(act);
    }

    [Fact]
    public void AddRedirection_ShouldAddRedirection_WhenValidAliasAndTokenProvided()
    {
        // Arrange
        var alias = "alias1";
        var token = "secret jwt token";
        var tokenEncoded = Uri.EscapeDataString(token);
        var expectedUrl = $"/select?token={tokenEncoded}";

        // Act
        _testee.AddRedirection(alias, token);
        var redirection = _testee.GetTargetOfRedirection(alias);

        // Assert
        redirection.ShouldNotBeNull();
        redirection.ShouldBe(expectedUrl);
    }

    [Fact]
    public void WhenTokenGetsInvalidated_RedirectionShouldBeRemoved()
    {
        // Arrange
        var alias = "alias1";
        var token = "secret jwt token";
        _testee.AddRedirection(alias, token);

        // Act
        _tokenCreator.InvokeOnTokenInvalidated(token);

        // Assert
        Should.Throw<KeyNotFoundException>(() => _testee.GetTargetOfRedirection(alias));
    }
    
    [Fact]
    public void WhenOtherTokenGetsInvalidated_RedirectionShouldBeNotRemoved()
    {
        // Arrange
        var alias = "alias1";
        var token = "secret jwt token";
        var otherToken = "other token";
        _testee.AddRedirection(alias, token);

        // Act
        _tokenCreator.InvokeOnTokenInvalidated(otherToken);

        // Assert
        Should.NotThrow(() => _testee.GetTargetOfRedirection(alias));
    }
}

file static class TokenCreatorExtensions
{
    internal static void InvokeOnTokenInvalidated(this TokenCreator @this, string token)
    {
        const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        if (typeof(TokenCreator).GetField("OnTokenInvalidated", bindingFlags) is not { } field) return;
        var @event = field.GetValue(@this) as Action<string>;
        @event?.Invoke(token);
    }
}