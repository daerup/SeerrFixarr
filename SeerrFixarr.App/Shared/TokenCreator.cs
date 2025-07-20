using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.Runners;
using SeerrFixarr.App.Runners.Sonarr;

namespace SeerrFixarr.App.Shared;

public record TokenData(int IssueId, int MediaId, IssueTarget IssueTarget, string UserLocale);

public enum IssueTarget
{
    Movie,
    Episode,
    Season,
    Show
}

public class TokenCreator(TimeProvider timeProvider, string secret)
{
    private readonly Lock _lock = new();
    private readonly HashSet<string> _revokedTokens = [];
    private const string IssueIdClaimString = nameof(IssueIdClaimString);
    private const string MediaIdClaimString = nameof(MediaIdClaimString);
    private const string IssueTargetClaimString = nameof(IssueTargetClaimString);
    private const string UserLocaleClaimString = nameof(UserLocaleClaimString);

    public event Action<string> OnTokenRevoked = delegate { };

    public string CreateToken(int issueId, IssueTargetInformation issueInfo, TimeSpan expiresIn, string locale)
    {
        var (targetType, targetId) = issueInfo;
        var claims = new[]
        {
            new Claim(IssueIdClaimString, issueId.ToString()),
            new Claim(MediaIdClaimString, targetId.ToString()),
            new Claim(IssueTargetClaimString, targetType.ToString()),
            new Claim(UserLocaleClaimString, locale)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var dateTime = timeProvider.GetUtcNow().UtcDateTime;
        var token = new JwtSecurityToken(
            claims: claims,
            expires: dateTime.Add(expiresIn),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public void RevokeToken(string token)
    {
        lock (_lock)
        {
            _revokedTokens.Add(token);
            OnTokenRevoked(token);
        }
    }

    private bool IsRevoked(string token)
    {
        lock (_lock)
        {
            return _revokedTokens.Contains(token);
        }
    }

    private void RefreshRevokedTokens()
    {
        var now = timeProvider.GetUtcNow().DateTime;
        lock (_lock)
        {
            _revokedTokens.RemoveWhere(token =>
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var expired = jwtToken.ValidTo < now;
                    if (expired)
                    {
                        OnTokenRevoked(token);
                    }

                    return expired;
                }
                catch
                {
                    // If the token is invalid, we can safely remove it
                    return true;
                }
            });
        }
    }

    public bool TryValidateToken(string token, out TokenData tokenData)
    {
        RefreshRevokedTokens();
        if (IsRevoked(token))
        {
            tokenData = null!;
            return false;
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var tokenValidationParameters = CreateTokenValidationParameters();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);

            var issueIdClaim = principal.FindFirst(IssueIdClaimString)?.Value;
            var mediaIdClaim = principal.FindFirst(MediaIdClaimString)?.Value;
            var targetClaim = principal.FindFirst(IssueTargetClaimString)?.Value;
            var localeClaim = principal.FindFirst(UserLocaleClaimString)?.Value;

            if (int.TryParse(issueIdClaim, out var issueId) &&
                int.TryParse(mediaIdClaim, out var mediaId) &&
                Enum.TryParse<IssueTarget>(targetClaim, out var issueTarget) &&
                localeClaim is { } locale)
            {
                tokenData = new TokenData(issueId, mediaId, issueTarget, locale);
                return true;
            }

            tokenData = null!;
            return false;
        }
        catch
        {
            tokenData = null!;
            return false;
        }
    }

    private TokenValidationParameters CreateTokenValidationParameters()
    {
        var key = Encoding.UTF8.GetBytes(secret);
        return new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero,
        }.WithTimeProvider(timeProvider);
    }
}