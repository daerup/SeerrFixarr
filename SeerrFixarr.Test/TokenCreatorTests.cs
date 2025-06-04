using Microsoft.Extensions.Time.Testing;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.Shared;
using SeerrFixarr.Test.Infrastructure;
using Shouldly;

namespace SeerrFixarr.Test;

public class TokenCreatorTests
{
    private const string _256BitSecret = "super-secret-key-used-for-signing";
    private readonly FakeTimeProvider _timeProvider;
    private readonly TokenCreator _testee;

    public TokenCreatorTests()
    {
        _timeProvider = new FakeTimeProvider();
        _testee = new TokenCreator(_timeProvider, _256BitSecret);
    }

    [Fact]
    public void CreateToken_ShouldGenerateToken()
    {
        // Arrange
        var id = 1;
        var mediaType = MediaType.Movie;
        var expiresIn = TimeSpan.FromHours(1);

        // Act
        var token = _testee.CreateToken(id, mediaType, expiresIn);

        // Assert
        token.ShouldNotBeNull();
        token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CreateToken_ShouldGenerateValidToken()
    {
        // Arrange
        var id = 1;
        var mediaType = MediaType.Movie;
        var expiresIn = TimeSpan.FromHours(1);
        var token = _testee.CreateToken(id, mediaType, expiresIn);

        // Act
        var claimsPrincipal = TestTokenValidator.TryValidateToken(token, _256BitSecret, _timeProvider, out _);

        // Assert
        claimsPrincipal.ShouldNotBeNull();
        claimsPrincipal.Claims.ShouldNotBeEmpty();
    }

    [Fact]
    public void InvalidateToken_ShouldMarkTokenAsRevoked()
    {
        // Arrange
        var id = 1;
        var mediaType = MediaType.Movie;
        var expiresIn = TimeSpan.FromHours(1);
        var token = _testee.CreateToken(id, mediaType, expiresIn);

        // Act
        _testee.RevokeToken(token);

        // Assert
        _testee.IsRevoked(token).ShouldBeTrue();
    }

    [Fact]
    public void TryValidateToken_ShouldReturnTrueForValidToken()
    {
        // Arrange
        var id = 1;
        var mediaType = MediaType.Movie;
        var expiresIn = TimeSpan.FromHours(1);
        var token = _testee.CreateToken(id, mediaType, expiresIn);

        // Act
        var isValid = _testee.TryValidateToken(token, out var tokenData);

        // Assert
        isValid.ShouldBeTrue();
        tokenData.ShouldNotBeNull();
        tokenData.Id.ShouldBe(id);
        tokenData.MediaType.ShouldBe(mediaType);
    }

    [Fact]
    public void TryValidateToken_ShouldReturnFalseForExpiredToken()
    {
        // Arrange
        var id = 1;
        var mediaType = MediaType.Movie;
        var expiresIn = TimeSpan.FromDays(1);
        var token = _testee.CreateToken(id, mediaType, expiresIn);

        // Act
        _timeProvider.Advance(expiresIn.Add(TimeSpan.FromDays(10)));
        var isValid = _testee.TryValidateToken(token, out var tokenData);
        
        // Assert
        isValid.ShouldBeFalse();
        tokenData.ShouldBeNull();
    }
    
    [Fact]
    public void TryValidateToken_ShouldReturnFalseForFaultySignature()
    {
        // Arrange
        var id = 1;
        var mediaType = MediaType.Movie;
        var expiresIn = TimeSpan.FromHours(1);
        var token = _testee.CreateToken(id, mediaType, expiresIn);

        // Act
        var faultyToken = token + "faulty-signature";
        var isValid = _testee.TryValidateToken(faultyToken, out var tokenData);

        // Assert
        isValid.ShouldBeFalse();
        tokenData.ShouldBeNull();
    }

    [Fact]
    public void TryValidateToken_ShouldReturnFalseForRevokedToken()
    {
        // Arrange
        var id = 1;
        var mediaType = MediaType.Movie;
        var expiresIn = TimeSpan.FromHours(1);
        var token = _testee.CreateToken(id, mediaType, expiresIn);
        _testee.RevokeToken(token);

        // Act
        var isValid = _testee.TryValidateToken(token, out var tokenData);

        // Assert
        isValid.ShouldBeFalse();
        tokenData.ShouldBeNull();
    }

    [Fact]
    public void InvalidateToken_ShouldTriggerOnTokenInvalidated()
    {
        // Arrange
        var token = "dummy-token";
        var eventTriggered = false;
        _testee.OnTokenInvalidated += _ => eventTriggered = true;

        // Act
        _testee.RevokeToken(token);

        // Assert
        eventTriggered.ShouldBeTrue();
    }

    [Fact]
    public void TryValidateToken_ShouldTriggerOnTokenInvalidatedForExpiredTokens()
    {
        // Arrange
        var id = 1;
        var mediaType = MediaType.Movie;
        var expiresIn = TimeSpan.FromDays(1);
        var token = _testee.CreateToken(id, mediaType, expiresIn);
        _testee.RevokeToken(token);
        var eventTriggered = false;
        _testee.OnTokenInvalidated += _ => eventTriggered = true;

        // Act
        _timeProvider.Advance(TimeSpan.FromDays(10));
        var isValid = _testee.TryValidateToken(token, out var tokenData);

        // Assert
        isValid.ShouldBeFalse();
        tokenData.ShouldBeNull();
        eventTriggered.ShouldBeTrue();
    }
}