using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SeerrFixarr.Api.Overseerr;


internal record TokenData(int Id, MediaType MediaType);

internal class TokenCreator(string secret)
{
    private readonly object _lock = new();
    private readonly HashSet<string> _revokedTokens = [];
    private const string IdClaimString = "id";
    private const string MediaTypeClaimString = "mediaType";

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
            expires: DateTime.UtcNow.Add(expiresIn),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public void InvalidateToken(string token)
    {
        lock (_lock)
        {
            _revokedTokens.Add(token);
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
        var now = DateTime.UtcNow;
        lock (_lock)
        {
            _revokedTokens.RemoveWhere(token =>
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    return jwtToken.ValidTo < now;
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
        var key = Encoding.UTF8.GetBytes(secret);
        
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false ,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out _);

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
}