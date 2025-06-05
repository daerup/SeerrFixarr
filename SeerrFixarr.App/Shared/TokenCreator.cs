using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SeerrFixarr.Api.Overseerr;

namespace SeerrFixarr.App.Shared;

public record TokenData(int Id, MediaType MediaType);

public class TokenCreator(TimeProvider timeProvider, string secret)
{
    private readonly Lock _lock = new();
    private readonly HashSet<string> _revokedTokens = [];
    private const string IdClaimString = "id";
    private const string MediaTypeClaimString = "mediaType";

    public event Action<string> OnTokenRevoked = delegate { };
    
    public string CreateToken(int id, MediaType mediaType, TimeSpan expiresIn)
    {
        var claims = new[]
        {
            new Claim(IdClaimString, id.ToString()),
            new Claim(MediaTypeClaimString, mediaType.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            claims: claims,
            expires: timeProvider.GetUtcNow().Add(expiresIn).DateTime,
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
            
            var idClaim = principal.FindFirst(IdClaimString)?.Value;
            var typeClaim = principal.FindFirst(MediaTypeClaimString)?.Value;

            if (int.TryParse(idClaim, out var id) && Enum.TryParse<MediaType>(typeClaim, out var mediaType))
            {
                tokenData = new TokenData(id, mediaType);
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
            ValidateIssuer = false ,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero,
        }.WithTimeProvider(timeProvider);
    }
}